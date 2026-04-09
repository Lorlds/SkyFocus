using CommunityToolkit.Mvvm.ComponentModel;
using SkyFocus.Models;
using SkyFocus.Services;

namespace SkyFocus.ViewModels;

internal partial class SettingsPageViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    public SettingsPageViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [ObservableProperty]
    public partial int DefaultSessionMinutes { get; set; } = 25;

    [ObservableProperty]
    public partial bool EnableSoftBlocker { get; set; } = true;

    [ObservableProperty]
    public partial bool UseFocusShield { get; set; } = true;

    [ObservableProperty]
    public partial bool KeepWindowOnTop { get; set; }

    [ObservableProperty]
    public partial bool SendSystemNotifications { get; set; } = true;

    [ObservableProperty]
    public partial bool NotifyAtFiveMinutes { get; set; } = true;

    [ObservableProperty]
    public partial string SelectedTheme { get; set; } = "System";

    public async Task InitializeAsync()
    {
        AppSettings settings = await _settingsService.GetAsync().ConfigureAwait(false);
        ApplySettings(settings);
    }

    public async Task RestoreDefaultsAsync()
    {
        AppSettings defaults = new();
        ApplySettings(defaults);
        await _settingsService.SaveAsync(defaults).ConfigureAwait(false);
    }

    public async Task SaveAsync()
    {
        AppSettings current = await _settingsService.GetAsync().ConfigureAwait(false);
        AppSettings updated = current with
        {
            DefaultSessionMinutes = Math.Clamp(DefaultSessionMinutes, 10, 180),
            Theme = SelectedTheme,
            EnableSoftBlocker = EnableSoftBlocker,
            UseFocusShield = UseFocusShield,
            KeepWindowOnTop = KeepWindowOnTop,
            SendSystemNotifications = SendSystemNotifications,
            NotifyAtFiveMinutes = NotifyAtFiveMinutes,
        };

        await _settingsService.SaveAsync(updated).ConfigureAwait(false);
    }

    private void ApplySettings(AppSettings settings)
    {
        DefaultSessionMinutes = settings.DefaultSessionMinutes;
        EnableSoftBlocker = settings.EnableSoftBlocker;
        UseFocusShield = settings.UseFocusShield;
        KeepWindowOnTop = settings.KeepWindowOnTop;
        SendSystemNotifications = settings.SendSystemNotifications;
        NotifyAtFiveMinutes = settings.NotifyAtFiveMinutes;
        SelectedTheme = settings.Theme;
    }
}
