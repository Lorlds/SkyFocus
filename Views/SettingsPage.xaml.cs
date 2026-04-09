using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkyFocus.ViewModels;

namespace SkyFocus.Views;

public sealed partial class SettingsPage : Page
{
    private bool _initialized;

    internal SettingsPage(SettingsPageViewModel viewModel)
    {
        ViewModel = viewModel;
        SessionPresets = new ObservableCollection<int>([15, 25, 45, 60, 90]);
        ThemeOptions = new ObservableCollection<string>(["System", "Light", "Dark"]);
        InitializeComponent();
        Loaded += OnLoaded;
    }

    internal SettingsPageViewModel ViewModel { get; }

    public ObservableCollection<int> SessionPresets { get; }

    public ObservableCollection<string> ThemeOptions { get; }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await ViewModel.InitializeAsync();
    }

    private async void OnRestoreClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.RestoreDefaultsAsync();
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveAsync();
    }
}