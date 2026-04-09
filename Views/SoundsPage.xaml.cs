using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using SkyFocus.ViewModels;

namespace SkyFocus.Views;

public sealed partial class SoundsPage : Page
{
    private bool _initialized;

    internal SoundsPage(SoundsPageViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    internal SoundsPageViewModel ViewModel { get; }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await ViewModel.InitializeAsync();
    }

    private async void OnPlayClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.PlaySelectedAsync();
    }

    private async void OnSelectedProfileChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_initialized)
        {
            return;
        }

        await ViewModel.SaveSelectionAsync();
    }

    private async void OnStopClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.StopAsync();
    }

    private async void OnVolumeChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (!_initialized)
        {
            return;
        }

        await ViewModel.UpdateVolumeAsync();
    }
}