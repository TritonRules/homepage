namespace Inicio.IptvPlayer.Wpf.Domain;

public sealed class AudioTrackItem
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;

    public override string ToString() => Name;
}
