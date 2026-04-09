using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkyFocus.Services;
using SkyFocus.ViewModels;

namespace SkyFocus.Views;

public sealed partial class HomePage : Page
{
    private readonly INavigationService _navigationService;
    private bool _initialized;

    internal HomePage(HomePageViewModel viewModel, INavigationService navigationService)
    {
        ViewModel = viewModel;
        _navigationService = navigationService;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    internal HomePageViewModel ViewModel { get; }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await ViewModel.InitializeAsync();
    }

    private void OnOpenFocusClick(object sender, RoutedEventArgs e)
    {
        _navigationService.NavigateTo<FocusPage>();
    }
}