namespace Inicio.IptvPlayer.Wpf.Domain;

public sealed class EpgNowNext
{
    public EpgProgram? Current { get; init; }
    public EpgProgram? Next { get; init; }
}
