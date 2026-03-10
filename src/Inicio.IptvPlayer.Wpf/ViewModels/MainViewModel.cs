using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Inicio.IptvPlayer.Wpf.Data;
using Inicio.IptvPlayer.Wpf.Domain;
using Inicio.IptvPlayer.Wpf.Services;
using Microsoft.Win32;

namespace Inicio.IptvPlayer.Wpf.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const int PageSize = 500;

    private readonly IIptvRepository _repository;
    private readonly M3uImporter _m3uImporter;
    private readonly XmlTvImporter _xmlTvImporter;
    private readonly IPlayerService _playerService;

    private int _offset;

    public MainViewModel(
        IIptvRepository repository,
        M3uImporter m3uImporter,
        XmlTvImporter xmlTvImporter,
        IPlayerService playerService)
    {
        _repository = repository;
        _m3uImporter = m3uImporter;
        _xmlTvImporter = xmlTvImporter;
        _playerService = playerService;

        Groups = new ObservableCollection<string>();
        Channels = new ObservableCollection<ChannelItem>();
        AudioTracks = new ObservableCollection<AudioTrackItem>();

        SelectedGroup = "Todos";
    }

    public ObservableCollection<string> Groups { get; }
    public ObservableCollection<ChannelItem> Channels { get; }
    public ObservableCollection<AudioTrackItem> AudioTracks { get; }

    public LibVLCSharp.Shared.MediaPlayer Player => _playerService.MediaPlayer;

    [ObservableProperty]
    private ChannelItem? selectedChannel;

    [ObservableProperty]
    private AudioTrackItem? selectedAudioTrack;

    [ObservableProperty]
    private string? selectedGroup;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string nowProgram = "Sin EPG actual";

    [ObservableProperty]
    private string nextProgram = "Sin siguiente programa";

    [ObservableProperty]
    private string status = "Listo";

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private long totalChannels;

    partial void OnSelectedGroupChanged(string? value)
    {
        _ = ReloadChannelsAsync();
    }

    partial void OnSelectedChannelChanged(ChannelItem? value)
    {
        _ = PlaySelectedChannelAsync();
    }

    partial void OnSelectedAudioTrackChanged(AudioTrackItem? value)
    {
        if (value is not null)
        {
            _playerService.SetAudioTrack(value.Id);
        }
    }

    [RelayCommand]
    private async Task ImportM3uAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "M3U files|*.m3u;*.m3u8|All files|*.*"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            Status = "Importando lista M3U...";
            await _m3uImporter.ImportAsync(dialog.FileName);
            await LoadGroupsAsync();
            await ReloadChannelsAsync();
            Status = "Lista M3U importada";
        });
    }

    [RelayCommand]
    private async Task ImportXmlTvAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "XMLTV files|*.xml;*.xmltv;*.xml.gz;*.xmltv.gz|All files|*.*"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            Status = "Importando XMLTV...";
            await _xmlTvImporter.ImportAsync(dialog.FileName);
            Status = "Guía EPG importada";
            await LoadEpgNowNextAsync();
        });
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await ReloadChannelsAsync();
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        var items = await _repository.SearchChannelsAsync(SelectedGroup, SearchText, PageSize, _offset);
        foreach (var item in items)
        {
            Channels.Add(item);
        }

        _offset += items.Count;
    }

    [RelayCommand]
    private void StopPlayback()
    {
        _playerService.Stop();
        AudioTracks.Clear();
    }

    public async Task InitializeAsync()
    {
        await LoadGroupsAsync();
        await ReloadChannelsAsync();
    }

    private async Task ReloadChannelsAsync()
    {
        _offset = 0;
        Channels.Clear();
        TotalChannels = await _repository.CountChannelsAsync(SelectedGroup, SearchText);
        await LoadMoreAsync();
    }

    private async Task LoadGroupsAsync()
    {
        var groups = await _repository.GetGroupsAsync();

        Groups.Clear();
        foreach (var group in groups)
        {
            Groups.Add(group);
        }

        if (!Groups.Contains(SelectedGroup))
        {
            SelectedGroup = Groups.FirstOrDefault() ?? "Todos";
        }
    }

    private async Task PlaySelectedChannelAsync()
    {
        if (SelectedChannel is null)
        {
            return;
        }

        try
        {
            _playerService.Play(SelectedChannel.StreamUrl);
            await Task.Delay(500);
            await RefreshAudioTracksAsync();
            await LoadEpgNowNextAsync();
            Status = $"Reproduciendo: {SelectedChannel.Name}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No fue posible reproducir el canal. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadEpgNowNextAsync()
    {
        if (SelectedChannel?.TvgId is null)
        {
            NowProgram = "Sin EPG actual";
            NextProgram = "Sin siguiente programa";
            return;
        }

        var epg = await _repository.GetNowNextAsync(SelectedChannel.TvgId, DateTimeOffset.UtcNow);

        NowProgram = epg.Current is null
            ? "Sin EPG actual"
            : $"Ahora: {epg.Current.Title} ({epg.Current.StartUtc:HH:mm} - {epg.Current.EndUtc:HH:mm})";

        NextProgram = epg.Next is null
            ? "Sin siguiente programa"
            : $"Siguiente: {epg.Next.Title} ({epg.Next.StartUtc:HH:mm} - {epg.Next.EndUtc:HH:mm})";
    }

    private Task RefreshAudioTracksAsync()
    {
        AudioTracks.Clear();
        foreach (var track in _playerService.GetAudioTracks())
        {
            AudioTracks.Add(track);
        }

        if (AudioTracks.Count > 0)
        {
            SelectedAudioTrack = AudioTracks[0];
        }

        return Task.CompletedTask;
    }

    private async Task RunBusyAsync(Func<Task> action)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await action();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
