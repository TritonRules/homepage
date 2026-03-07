using Inicio.IptvPlayer.Wpf.Domain;
using LibVLCSharp.Shared;

namespace Inicio.IptvPlayer.Wpf.Services;

public sealed class VlcPlayerService : IPlayerService
{
    private readonly LibVLC _libVlc;
    public MediaPlayer MediaPlayer { get; }

    public VlcPlayerService()
    {
        _libVlc = new LibVLC("--network-caching=1000", "--no-video-title-show");
        MediaPlayer = new MediaPlayer(_libVlc);
    }

    public void Play(string url)
    {
        using var media = new Media(_libVlc, new Uri(url));
        MediaPlayer.Play(media);
    }

    public void Stop() => MediaPlayer.Stop();

    public IReadOnlyList<AudioTrackItem> GetAudioTracks()
    {
        var tracks = MediaPlayer.AudioTrackDescription;
        if (tracks is null)
        {
            return [];
        }

        return tracks
            .Where(x => x.Id >= 0)
            .Select(x => new AudioTrackItem { Id = x.Id, Name = x.Name })
            .ToList();
    }

    public void SetAudioTrack(int trackId)
    {
        MediaPlayer.SetAudioTrack(trackId);
    }

    public void Dispose()
    {
        MediaPlayer.Dispose();
        _libVlc.Dispose();
    }
}
