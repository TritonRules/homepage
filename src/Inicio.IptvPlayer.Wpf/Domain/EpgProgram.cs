namespace Inicio.IptvPlayer.Wpf.Domain;

public sealed class EpgProgram
{
    public long Id { get; init; }
    public string ChannelId { get; init; } = string.Empty;
    public DateTimeOffset StartUtc { get; init; }
    public DateTimeOffset EndUtc { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
}
