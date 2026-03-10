using Inicio.IptvPlayer.Wpf.Domain;
using LibVLCSharp.Shared;

namespace Inicio.IptvPlayer.Wpf.Services;

public interface IPlayerService : IDisposable
{
    MediaPlayer MediaPlayer { get; }
    void Play(string url);
    void Stop();
    IReadOnlyList<AudioTrackItem> GetAudioTracks();
    void SetAudioTrack(int trackId);
}
