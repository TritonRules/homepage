using Microsoft.Data.Sqlite;

namespace Inicio.IptvPlayer.Wpf.Data;

public sealed class SqliteDatabase
{
    private readonly string _connectionString;

    public SqliteDatabase(string databasePath)
    {
        var dir = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        }.ToString();
    }

    public SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public void Initialize()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS Channels (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    GroupName TEXT NOT NULL,
    StreamUrl TEXT NOT NULL,
    TvgId TEXT NULL,
    LogoUrl TEXT NULL
);

CREATE TABLE IF NOT EXISTS EpgPrograms (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ChannelId TEXT NOT NULL,
    StartUtc INTEGER NOT NULL,
    EndUtc INTEGER NOT NULL,
    Title TEXT NOT NULL,
    Description TEXT NULL
);

CREATE INDEX IF NOT EXISTS IX_Channels_Group ON Channels (GroupName);
CREATE INDEX IF NOT EXISTS IX_Channels_Name ON Channels (Name);
CREATE INDEX IF NOT EXISTS IX_Channels_TvgId ON Channels (TvgId);
CREATE INDEX IF NOT EXISTS IX_EpgPrograms_Channel_Start ON EpgPrograms (ChannelId, StartUtc);
";
        command.ExecuteNonQuery();
    }
}
