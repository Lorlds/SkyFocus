using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkyFocus.ViewModels;

namespace SkyFocus.Views;

public sealed partial class HistoryPage : Page
{
    private bool _initialized;

    internal HistoryPage(HistoryPageViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    internal HistoryPageViewModel ViewModel { get; }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await ViewModel.InitializeAsync();
    }
}