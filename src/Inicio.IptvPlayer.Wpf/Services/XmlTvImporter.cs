using System.IO.Compression;
using System.Xml;
using Inicio.IptvPlayer.Wpf.Data;
using Inicio.IptvPlayer.Wpf.Domain;

namespace Inicio.IptvPlayer.Wpf.Services;

public sealed class XmlTvImporter
{
    private readonly IIptvRepository _repository;

    public XmlTvImporter(IIptvRepository repository)
    {
        _repository = repository;
    }

    public async Task ImportAsync(string path, CancellationToken cancellationToken = default)
    {
        await _repository.ReplaceEpgAsync(ParsePrograms(path, cancellationToken), cancellationToken);
    }

    private static async IAsyncEnumerable<EpgProgram> ParsePrograms(
        string path,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Yield();

        using var file = File.OpenRead(path);
        Stream source = path.EndsWith(".gz", StringComparison.OrdinalIgnoreCase)
            ? new GZipStream(file, CompressionMode.Decompress)
            : file;

        var settings = new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            IgnoreComments = true,
            Async = false
        };

        using var reader = XmlReader.Create(source, settings);

        while (reader.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (reader.NodeType != XmlNodeType.Element || !reader.Name.Equals("programme", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var channelId = reader.GetAttribute("channel") ?? string.Empty;
            var start = ParseXmlTvDate(reader.GetAttribute("start"));
            var stop = ParseXmlTvDate(reader.GetAttribute("stop"));

            string title = string.Empty;
            string? description = null;

            if (reader.IsEmptyElement)
            {
                continue;
            }

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals("programme", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (reader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                if (reader.Name.Equals("title", StringComparison.OrdinalIgnoreCase))
                {
                    title = reader.ReadElementContentAsString();
                    continue;
                }

                if (reader.Name.Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    description = reader.ReadElementContentAsString();
                }
            }

            if (!string.IsNullOrWhiteSpace(channelId) && start is not null && stop is not null && !string.IsNullOrWhiteSpace(title))
            {
                yield return new EpgProgram
                {
                    ChannelId = channelId,
                    StartUtc = start.Value.ToUniversalTime(),
                    EndUtc = stop.Value.ToUniversalTime(),
                    Title = title,
                    Description = description
                };
            }
        }
    }

    private static DateTimeOffset? ParseXmlTvDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        raw = raw.Trim();
        var datePart = raw.Length >= 14 ? raw[..14] : raw;

        if (!DateTime.TryParseExact(datePart, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.AssumeUniversal, out var dt))
        {
            return null;
        }

        return new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc));
    }
}
