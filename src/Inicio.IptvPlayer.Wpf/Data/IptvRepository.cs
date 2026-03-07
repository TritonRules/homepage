using Inicio.IptvPlayer.Wpf.Domain;
using Microsoft.Data.Sqlite;

namespace Inicio.IptvPlayer.Wpf.Data;

public sealed class IptvRepository : IIptvRepository
{
    private readonly SqliteDatabase _database;

    public IptvRepository(SqliteDatabase database)
    {
        _database = database;
    }

    public async Task ReplaceChannelsAsync(IEnumerable<ChannelItem> channels, CancellationToken cancellationToken = default)
    {
        await using var connection = _database.OpenConnection();
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var deleteCmd = connection.CreateCommand();
        deleteCmd.Transaction = transaction;
        deleteCmd.CommandText = "DELETE FROM Channels";
        await deleteCmd.ExecuteNonQueryAsync(cancellationToken);

        var insertCmd = connection.CreateCommand();
        insertCmd.Transaction = transaction;
        insertCmd.CommandText = @"
INSERT INTO Channels (Name, GroupName, StreamUrl, TvgId, LogoUrl)
VALUES ($name, $group, $url, $tvgId, $logo)";
        insertCmd.Parameters.Add("$name", SqliteType.Text);
        insertCmd.Parameters.Add("$group", SqliteType.Text);
        insertCmd.Parameters.Add("$url", SqliteType.Text);
        insertCmd.Parameters.Add("$tvgId", SqliteType.Text);
        insertCmd.Parameters.Add("$logo", SqliteType.Text);

        foreach (var channel in channels)
        {
            cancellationToken.ThrowIfCancellationRequested();
            insertCmd.Parameters["$name"].Value = channel.Name;
            insertCmd.Parameters["$group"].Value = channel.GroupName;
            insertCmd.Parameters["$url"].Value = channel.StreamUrl;
            insertCmd.Parameters["$tvgId"].Value = channel.TvgId ?? (object)DBNull.Value;
            insertCmd.Parameters["$logo"].Value = channel.LogoUrl ?? (object)DBNull.Value;
            await insertCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetGroupsAsync(CancellationToken cancellationToken = default)
    {
        var groups = new List<string> { "Todos" };
        await using var connection = _database.OpenConnection();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT DISTINCT GroupName FROM Channels ORDER BY GroupName";

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            groups.Add(reader.GetString(0));
        }

        return groups;
    }

    public async Task<IReadOnlyList<ChannelItem>> SearchChannelsAsync(string? group, string? search, int limit, int offset, CancellationToken cancellationToken = default)
    {
        await using var connection = _database.OpenConnection();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
SELECT Id, Name, GroupName, StreamUrl, TvgId, LogoUrl
FROM Channels
WHERE ($group IS NULL OR GroupName = $group)
  AND ($search IS NULL OR Name LIKE '%' || $search || '%')
ORDER BY Name
LIMIT $limit OFFSET $offset";

        cmd.Parameters.AddWithValue("$group", group is null || group == "Todos" ? DBNull.Value : group);
        cmd.Parameters.AddWithValue("$search", string.IsNullOrWhiteSpace(search) ? DBNull.Value : search.Trim());
        cmd.Parameters.AddWithValue("$limit", limit);
        cmd.Parameters.AddWithValue("$offset", offset);

        var result = new List<ChannelItem>(limit);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new ChannelItem
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(1),
                GroupName = reader.GetString(2),
                StreamUrl = reader.GetString(3),
                TvgId = reader.IsDBNull(4) ? null : reader.GetString(4),
                LogoUrl = reader.IsDBNull(5) ? null : reader.GetString(5)
            });
        }

        return result;
    }

    public async Task<long> CountChannelsAsync(string? group, string? search, CancellationToken cancellationToken = default)
    {
        await using var connection = _database.OpenConnection();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
SELECT COUNT(*)
FROM Channels
WHERE ($group IS NULL OR GroupName = $group)
  AND ($search IS NULL OR Name LIKE '%' || $search || '%')";

        cmd.Parameters.AddWithValue("$group", group is null || group == "Todos" ? DBNull.Value : group);
        cmd.Parameters.AddWithValue("$search", string.IsNullOrWhiteSpace(search) ? DBNull.Value : search.Trim());

        return (long)(await cmd.ExecuteScalarAsync(cancellationToken) ?? 0L);
    }

    public async Task ReplaceEpgAsync(IAsyncEnumerable<EpgProgram> programs, CancellationToken cancellationToken = default)
    {
        await using var connection = _database.OpenConnection();
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var deleteCmd = connection.CreateCommand();
        deleteCmd.Transaction = transaction;
        deleteCmd.CommandText = "DELETE FROM EpgPrograms";
        await deleteCmd.ExecuteNonQueryAsync(cancellationToken);

        var insertCmd = connection.CreateCommand();
        insertCmd.Transaction = transaction;
        insertCmd.CommandText = @"
INSERT INTO EpgPrograms (ChannelId, StartUtc, EndUtc, Title, Description)
VALUES ($channelId, $startUtc, $endUtc, $title, $description)";
        insertCmd.Parameters.Add("$channelId", SqliteType.Text);
        insertCmd.Parameters.Add("$startUtc", SqliteType.Integer);
        insertCmd.Parameters.Add("$endUtc", SqliteType.Integer);
        insertCmd.Parameters.Add("$title", SqliteType.Text);
        insertCmd.Parameters.Add("$description", SqliteType.Text);

        await foreach (var program in programs.WithCancellation(cancellationToken))
        {
            insertCmd.Parameters["$channelId"].Value = program.ChannelId;
            insertCmd.Parameters["$startUtc"].Value = program.StartUtc.ToUnixTimeSeconds();
            insertCmd.Parameters["$endUtc"].Value = program.EndUtc.ToUnixTimeSeconds();
            insertCmd.Parameters["$title"].Value = program.Title;
            insertCmd.Parameters["$description"].Value = program.Description ?? (object)DBNull.Value;
            await insertCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<EpgNowNext> GetNowNextAsync(string tvgId, DateTimeOffset referenceUtc, CancellationToken cancellationToken = default)
    {
        await using var connection = _database.OpenConnection();

        var now = await GetCurrentProgramAsync(connection, tvgId, referenceUtc, cancellationToken);
        var next = await GetNextProgramAsync(connection, tvgId, referenceUtc, cancellationToken);

        return new EpgNowNext { Current = now, Next = next };
    }

    private static async Task<EpgProgram?> GetCurrentProgramAsync(SqliteConnection connection, string tvgId, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
SELECT Id, ChannelId, StartUtc, EndUtc, Title, Description
FROM EpgPrograms
WHERE ChannelId = $channelId
  AND StartUtc <= $now
  AND EndUtc > $now
ORDER BY StartUtc DESC
LIMIT 1";
        cmd.Parameters.AddWithValue("$channelId", tvgId);
        cmd.Parameters.AddWithValue("$now", nowUtc.ToUnixTimeSeconds());

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapProgram(reader) : null;
    }

    private static async Task<EpgProgram?> GetNextProgramAsync(SqliteConnection connection, string tvgId, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
SELECT Id, ChannelId, StartUtc, EndUtc, Title, Description
FROM EpgPrograms
WHERE ChannelId = $channelId
  AND StartUtc >= $now
ORDER BY StartUtc ASC
LIMIT 1";
        cmd.Parameters.AddWithValue("$channelId", tvgId);
        cmd.Parameters.AddWithValue("$now", nowUtc.ToUnixTimeSeconds());

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapProgram(reader) : null;
    }

    private static EpgProgram MapProgram(SqliteDataReader reader)
    {
        return new EpgProgram
        {
            Id = reader.GetInt64(0),
            ChannelId = reader.GetString(1),
            StartUtc = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(2)),
            EndUtc = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(3)),
            Title = reader.GetString(4),
            Description = reader.IsDBNull(5) ? null : reader.GetString(5)
        };
    }
}
