namespace Inicio.IptvPlayer.Wpf.Domain;

public sealed class ChannelItem
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string GroupName { get; init; } = "Otros";
    public string StreamUrl { get; init; } = string.Empty;
    public string? TvgId { get; init; }
    public string? LogoUrl { get; init; }
}
