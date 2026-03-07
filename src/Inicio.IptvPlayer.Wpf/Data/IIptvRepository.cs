using Inicio.IptvPlayer.Wpf.Domain;

namespace Inicio.IptvPlayer.Wpf.Data;

public interface IIptvRepository
{
    Task ReplaceChannelsAsync(IEnumerable<ChannelItem> channels, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetGroupsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChannelItem>> SearchChannelsAsync(string? group, string? search, int limit, int offset, CancellationToken cancellationToken = default);
    Task<long> CountChannelsAsync(string? group, string? search, CancellationToken cancellationToken = default);

    Task ReplaceEpgAsync(IAsyncEnumerable<EpgProgram> programs, CancellationToken cancellationToken = default);
    Task<EpgNowNext> GetNowNextAsync(string tvgId, DateTimeOffset referenceUtc, CancellationToken cancellationToken = default);
}
