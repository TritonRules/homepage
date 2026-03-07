using System.IO;
using System.Windows;
using Inicio.IptvPlayer.Wpf.Data;
using Inicio.IptvPlayer.Wpf.Services;
using Inicio.IptvPlayer.Wpf.ViewModels;
using LibVLCSharp.Shared;

namespace Inicio.IptvPlayer.Wpf;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Core.Initialize();

        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "InicioIptvPlayer",
            "iptv.db");

        var database = new SqliteDatabase(dbPath);
        database.Initialize();

        var repository = new IptvRepository(database);
        var playerService = new VlcPlayerService();
        var m3uImporter = new M3uImporter(repository);
        var xmlTvImporter = new XmlTvImporter(repository);

        Resources["MainViewModel"] = new MainViewModel(repository, m3uImporter, xmlTvImporter, playerService);

        base.OnStartup(e);
    }
}
