using System.Text.RegularExpressions;
using Inicio.IptvPlayer.Wpf.Data;
using Inicio.IptvPlayer.Wpf.Domain;

namespace Inicio.IptvPlayer.Wpf.Services;

public sealed class M3uImporter
{
    private static readonly Regex AttrRegex = new("(?<key>[a-zA-Z0-9\-]+)=\"(?<value>.*?)\"", RegexOptions.Compiled);
    private readonly IIptvRepository _repository;

    public M3uImporter(IIptvRepository repository)
    {
        _repository = repository;
    }

    public async Task ImportAsync(string path, CancellationToken cancellationToken = default)
    {
        var channels = Parse(path);
        await _repository.ReplaceChannelsAsync(channels, cancellationToken);
    }

    private static IEnumerable<ChannelItem> Parse(string path)
    {
        using var reader = new StreamReader(path);

        string? line;
        string currentName = string.Empty;
        string currentGroup = "Otros";
        string? currentTvgId = null;
        string? currentLogo = null;

        while ((line = reader.ReadLine()) is not null)
        {
            if (line.StartsWith("#EXTINF", StringComparison.OrdinalIgnoreCase))
            {
                var info = line.AsSpan();
                var comma = info.LastIndexOf(',');
                if (comma >= 0)
                {
                    currentName = line[(comma + 1)..].Trim();
                }

                var attrs = ParseAttributes(line);
                currentGroup = attrs.TryGetValue("group-title", out var group) && !string.IsNullOrWhiteSpace(group)
                    ? group
                    : "Otros";
                currentTvgId = attrs.TryGetValue("tvg-id", out var tvgId) ? tvgId : null;
                currentLogo = attrs.TryGetValue("tvg-logo", out var tvgLogo) ? tvgLogo : null;
            }
            else if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith('#'))
            {
                yield return new ChannelItem
                {
                    Name = string.IsNullOrWhiteSpace(currentName) ? "Sin nombre" : currentName,
                    GroupName = currentGroup,
                    StreamUrl = line.Trim(),
                    TvgId = currentTvgId,
                    LogoUrl = currentLogo
                };
            }
        }
    }

    private static Dictionary<string, string> ParseAttributes(string line)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in AttrRegex.Matches(line))
        {
            result[match.Groups["key"].Value] = match.Groups["value"].Value;
        }

        return result;
    }
}
