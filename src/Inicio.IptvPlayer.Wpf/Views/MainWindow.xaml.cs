using System.Windows;
using Inicio.IptvPlayer.Wpf.ViewModels;

namespace Inicio.IptvPlayer.Wpf.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = (MainViewModel)Application.Current.Resources["MainViewModel"];
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}
