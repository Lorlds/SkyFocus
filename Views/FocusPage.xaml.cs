using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkyFocus.ViewModels;

namespace SkyFocus.Views;

public sealed partial class FocusPage : Page
{
    private bool _initialized;

    internal FocusPage(FocusPageViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    internal FocusPageViewModel ViewModel { get; }

    private async void OnAbandonClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.AbandonAsync();
    }

    private async void OnCompleteClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.CompleteAsync();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await ViewModel.InitializeAsync();
    }

    private async void OnPauseClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.PauseAsync();
    }

    private async void OnResumeClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.ResumeAsync();
    }

    private async void OnStartClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.StartAsync();
    }
}