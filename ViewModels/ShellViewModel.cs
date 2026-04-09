using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using SkyFocus.Models;
using SkyFocus.Services;
using SkyFocus.Views;

namespace SkyFocus.ViewModels;

internal partial class ShellViewModel : ObservableObject
{
    private readonly IBlockerService _blockerService;
    private readonly IFocusSessionService _focusSessionService;
    private readonly INavigationService _navigationService;
    private readonly IReminderService _reminderService;
    private readonly ISettingsService _settingsService;
    private readonly ITextService _textService;

    public ShellViewModel(
        INavigationService navigationService,
        IFocusSessionService focusSessionService,
        IBlockerService blockerService,
        IReminderService reminderService,
        ISettingsService settingsService,
        ITextService textService)
    {
        _navigationService = navigationService;
        _focusSessionService = focusSessionService;
        _blockerService = blockerService;
        _reminderService = reminderService;
        _settingsService = settingsService;
        _textService = textService;

        _navigationService.CurrentPageChanged += OnCurrentPageChanged;
        _focusSessionService.SnapshotChanged += OnSnapshotChanged;
        _blockerService.StateChanged += OnBlockerStateChanged;
        _reminderService.ReminderRaised += OnReminderRaised;
    }

    public string AppTitle => _textService.GetString("AppDisplayName");

    public string HomeNavAutomationName => _textService.GetString("NavHomeAutomationName");

    public string FocusNavAutomationName => _textService.GetString("NavFocusAutomationName");

    public string HistoryNavAutomationName => _textService.GetString("NavHistoryAutomationName");

    public string SoundsNavAutomationName => _textService.GetString("NavSoundsAutomationName");

    public string SettingsNavAutomationName => _textService.GetString("NavSettingsAutomationName");

    public string ShieldReturnLabel => _textService.GetString("ShieldReturnActionLabel");

    public string ShieldDismissLabel => _textService.GetString("ShieldDismissActionLabel");

    [ObservableProperty]
    public partial Page? CurrentPage { get; set; }

    [ObservableProperty]
    public partial bool CanGoBack { get; set; }

    [ObservableProperty]
    public partial string CurrentDestinationKey { get; set; } = "home";

    [ObservableProperty]
    public partial string CurrentHeaderTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CurrentHeaderSubtitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FocusChipTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FocusChipSubtitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsShieldVisible { get; set; }

    [ObservableProperty]
    public partial string ShieldHeadline { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ShieldMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsReminderVisible { get; set; }

    [ObservableProperty]
    public partial string ReminderTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReminderMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial InfoBarSeverity ReminderSeverity { get; set; } = InfoBarSeverity.Informational;

    [ObservableProperty]
    public partial bool ShouldUseFullScreen { get; set; }

    [ObservableProperty]
    public partial bool ShouldStayOnTop { get; set; }

    public Task InitializeAsync()
    {
        if (_navigationService.CurrentPage is null)
        {
            _navigationService.NavigateTo<HomePage>();
        }

        UpdateFocusChip(_focusSessionService.Snapshot);
        UpdateBlockerState(_blockerService.State);
        return Task.CompletedTask;
    }

    public void DismissReminder()
    {
        _reminderService.Dismiss();
        IsReminderVisible = false;
        ReminderTitle = string.Empty;
        ReminderMessage = string.Empty;
    }

    public void DismissShield()
    {
        _blockerService.DismissShield();
        UpdateBlockerState(_blockerService.State);
    }

    public void GoBack()
    {
        _navigationService.GoBack();
    }

    public async Task<bool> NavigateAsync(string destinationKey)
    {
        AppSettings settings = await _settingsService.GetAsync().ConfigureAwait(false);
        if (!await _blockerService.CanNavigateAsync(destinationKey, _focusSessionService.Snapshot, settings).ConfigureAwait(false))
        {
            UpdateBlockerState(_blockerService.State);
            return false;
        }

        switch (destinationKey)
        {
            case "home":
                _navigationService.NavigateTo<HomePage>();
                break;
            case "focus":
                _navigationService.NavigateTo<FocusPage>();
                break;
            case "history":
                _navigationService.NavigateTo<HistoryPage>();
                break;
            case "sounds":
                _navigationService.NavigateTo<SoundsPage>();
                break;
            case "settings":
                _navigationService.NavigateTo<SettingsPage>();
                break;
            default:
                return false;
        }

        return true;
    }

    private void OnBlockerStateChanged(object? sender, BlockerState state)
    {
        UpdateBlockerState(state);
    }

    private void OnCurrentPageChanged(object? sender, Page? page)
    {
        CurrentPage = page;
        CanGoBack = _navigationService.CanGoBack;
        CurrentDestinationKey = page switch
        {
            HomePage => "home",
            FocusPage => "focus",
            HistoryPage => "history",
            SoundsPage => "sounds",
            SettingsPage => "settings",
            _ => CurrentDestinationKey,
        };

        (CurrentHeaderTitle, CurrentHeaderSubtitle) = CurrentDestinationKey switch
        {
            "focus" => (_textService.GetString("HeaderFocusTitle"), _textService.GetString("HeaderFocusSubtitle")),
            "history" => (_textService.GetString("HeaderHistoryTitle"), _textService.GetString("HeaderHistorySubtitle")),
            "sounds" => (_textService.GetString("HeaderSoundsTitle"), _textService.GetString("HeaderSoundsSubtitle")),
            "settings" => (_textService.GetString("HeaderSettingsTitle"), _textService.GetString("HeaderSettingsSubtitle")),
            _ => (_textService.GetString("HeaderHomeTitle"), _textService.GetString("HeaderHomeSubtitle")),
        };
    }

    private void OnReminderRaised(object? sender, ReminderMessage message)
    {
        ReminderTitle = message.Title;
        ReminderMessage = message.Body;
        ReminderSeverity = message.Severity;
        IsReminderVisible = true;
    }

    private void OnSnapshotChanged(object? sender, FocusSessionSnapshot snapshot)
    {
        UpdateFocusChip(snapshot);
    }

    private void UpdateBlockerState(BlockerState state)
    {
        IsShieldVisible = state.IsShieldVisible;
        ShieldHeadline = state.ShieldHeadline;
        ShieldMessage = state.ShieldMessage;
        ShouldUseFullScreen = state.ShouldUseFullScreen;
        ShouldStayOnTop = state.ShouldStayOnTop;
    }

    private void UpdateFocusChip(FocusSessionSnapshot snapshot)
    {
        switch (snapshot.Status)
        {
            case FocusSessionStatus.Running:
                FocusChipTitle = snapshot.RouteLabel;
                FocusChipSubtitle = _textService.Format("ShellFocusChipRunningFormat", FormatDuration(snapshot.Remaining));
                break;
            case FocusSessionStatus.Paused:
                FocusChipTitle = snapshot.RouteLabel;
                FocusChipSubtitle = _textService.GetString("ShellFocusChipPaused");
                break;
            case FocusSessionStatus.Completed:
                FocusChipTitle = snapshot.RouteLabel;
                FocusChipSubtitle = _textService.GetString("ShellFocusChipCompleted");
                break;
            case FocusSessionStatus.Abandoned:
                FocusChipTitle = snapshot.RouteLabel;
                FocusChipSubtitle = _textService.GetString("ShellFocusChipAbandoned");
                break;
            default:
                FocusChipTitle = _textService.GetString("ShellFocusChipIdleTitle");
                FocusChipSubtitle = _textService.GetString("ShellFocusChipIdleSubtitle");
                break;
        }
    }

    private static string FormatDuration(TimeSpan duration)
    {
        return duration.TotalHours >= 1 ? duration.ToString(@"hh\:mm\:ss") : duration.ToString(@"mm\:ss");
    }
}
