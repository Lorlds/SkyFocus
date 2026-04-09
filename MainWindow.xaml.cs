using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkyFocus.Models;
using SkyFocus.Services;
using SkyFocus.ViewModels;

namespace SkyFocus;

public sealed partial class MainWindow : Window
{
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoSize = 0x0001;
    private readonly IBlockerService _blockerService;
    private readonly IFocusSessionService _focusSessionService;
    private readonly IReminderService _reminderService;
    private readonly ISettingsService _settingsService;
    private readonly ITextService _textService;

    internal MainWindow(
        ShellViewModel viewModel,
        IBlockerService blockerService,
        IFocusSessionService focusSessionService,
        IReminderService reminderService,
        ISettingsService settingsService,
        ITextService textService)
    {
        ViewModel = viewModel;
        _blockerService = blockerService;
        _focusSessionService = focusSessionService;
        _reminderService = reminderService;
        _settingsService = settingsService;
        _textService = textService;

        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.SetIcon("Assets/AppIcon.ico");
        Activated += OnWindowActivated;
        _blockerService.StateChanged += OnBlockerStateChanged;
        _settingsService.SettingsChanged += OnSettingsChanged;
    }

    internal ShellViewModel ViewModel { get; }

    public object? FindName(string name) => RootLayout.FindName(name);

    public async Task InitializeAsync()
    {
        AppTitleBar.Title = ViewModel.AppTitle;
        AppWindow.Title = ViewModel.AppTitle;
        await ViewModel.InitializeAsync();
        await ApplyThemeAsync(await _settingsService.GetAsync());
        ApplyWindowPolicy(_blockerService.State);
        UpdateSelectedNavigationItem();
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

    private static readonly nint HwndTopMost = new(-1);

    private static readonly nint HwndNoTopMost = new(-2);

    private async Task ApplyThemeAsync(AppSettings settings)
    {
        RootLayout.RequestedTheme = settings.Theme switch
        {
            "Light" => ElementTheme.Light,
            "Dark" => ElementTheme.Dark,
            _ => ElementTheme.Default,
        };
    }

    private void ApplyWindowPolicy(BlockerState state)
    {
        if (state.ShouldUseFullScreen && AppWindow.Presenter.Kind != AppWindowPresenterKind.FullScreen)
        {
            AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        }
        else if (!state.ShouldUseFullScreen && AppWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen)
        {
            AppWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
        }

        nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        SetWindowPos(hwnd, state.ShouldStayOnTop ? HwndTopMost : HwndNoTopMost, 0, 0, 0, 0, SwpNoMove | SwpNoSize);
    }

    private async void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        ViewModel.GoBack();
        UpdateSelectedNavigationItem();
        await Task.CompletedTask;
    }

    private void OnBlockerStateChanged(object? sender, BlockerState state)
    {
        DispatcherQueue.TryEnqueue(() => ApplyWindowPolicy(state));
    }

    private void OnDismissShieldClick(object sender, RoutedEventArgs e)
    {
        ViewModel.DismissShield();
    }

    private async void OnNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer?.Tag is not string destinationKey)
        {
            return;
        }

        bool navigated = await ViewModel.NavigateAsync(destinationKey);
        if (!navigated)
        {
            UpdateSelectedNavigationItem();
            return;
        }

        UpdateSelectedNavigationItem();
    }

    private void OnReminderClosed(InfoBar sender, object args)
    {
        ViewModel.DismissReminder();
    }

    private async void OnReturnToFocusClick(object sender, RoutedEventArgs e)
    {
        ViewModel.DismissShield();
        await ViewModel.NavigateAsync("focus");
        UpdateSelectedNavigationItem();
    }

    private async void OnSettingsChanged(object? sender, AppSettings settings)
    {
        await ApplyThemeAsync(settings);
    }

    private async void OnWindowActivated(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState != WindowActivationState.Deactivated || _focusSessionService.Snapshot.Status != FocusSessionStatus.Running)
        {
            return;
        }

        AppSettings settings = await _settingsService.GetAsync();
        await _blockerService.ReportExternalInterruptionAsync(_focusSessionService.Snapshot, settings);
        await _reminderService.NotifySessionInterruptedAsync(_focusSessionService.Snapshot, settings.SendSystemNotifications);
    }

    private void UpdateSelectedNavigationItem()
    {
        ShellNavigationView.SelectedItem = ViewModel.CurrentDestinationKey switch
        {
            "focus" => FocusNavItem,
            "history" => HistoryNavItem,
            "sounds" => SoundsNavItem,
            "settings" => SettingsNavItem,
            _ => HomeNavItem,
        };
    }
}