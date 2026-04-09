using System.Globalization;
using Microsoft.UI.Xaml.Controls;
using SkyFocus.Models;
using SkyFocus.Services;

namespace SkyFocus.Tests.Support;

internal sealed class InMemorySessionRepository : ISessionRepository
{
    private readonly List<FocusSessionRecord> _records;

    public InMemorySessionRepository(IEnumerable<FocusSessionRecord>? seed = null)
    {
        _records = seed?.ToList() ?? [];
    }

    public IReadOnlyList<FocusSessionRecord> Records => _records;

    public Task AddSessionAsync(FocusSessionRecord record)
    {
        _records.Add(record);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<FocusSessionRecord>> GetRecentSessionsAsync(int count)
    {
        IReadOnlyList<FocusSessionRecord> recent = _records
            .OrderByDescending(record => record.EndedAtUtc)
            .Take(count)
            .ToList();
        return Task.FromResult(recent);
    }

    public Task<IReadOnlyList<FocusSessionRecord>> GetSessionsAsync(DateTimeOffset? fromUtc = null)
    {
        IReadOnlyList<FocusSessionRecord> sessions = _records
            .Where(record => fromUtc is null || record.EndedAtUtc >= fromUtc.Value)
            .OrderBy(record => record.EndedAtUtc)
            .ToList();
        return Task.FromResult(sessions);
    }

    public Task InitializeAsync() => Task.CompletedTask;
}

internal sealed class TestSettingsService : ISettingsService
{
    private AppSettings _current;

    public TestSettingsService(AppSettings? initial = null)
    {
        _current = initial ?? new AppSettings();
    }

    public event EventHandler<AppSettings>? SettingsChanged;

    public Task<AppSettings> GetAsync() => Task.FromResult(_current);

    public Task SaveAsync(AppSettings settings)
    {
        _current = settings;
        SettingsChanged?.Invoke(this, settings);
        return Task.CompletedTask;
    }

    public async Task<AppSettings> UpdateAsync(Func<AppSettings, AppSettings> update)
    {
        AppSettings next = update(await GetAsync());
        await SaveAsync(next);
        return next;
    }
}

internal sealed class TestReminderService : IReminderService
{
    public event EventHandler<ReminderMessage>? ReminderRaised;

    public ReminderMessage? Current { get; private set; }

    public int StartedCount { get; private set; }

    public int MilestoneCount { get; private set; }

    public int CompletedCount { get; private set; }

    public int InterruptedCount { get; private set; }

    public void Dismiss()
    {
        Current = null;
    }

    public Task NotifySessionCompletedAsync(FocusSessionSnapshot snapshot, bool sendSystemNotification)
    {
        CompletedCount++;
        Publish(new ReminderMessage("completed", snapshot.RouteLabel, InfoBarSeverity.Success));
        return Task.CompletedTask;
    }

    public Task NotifySessionInterruptedAsync(FocusSessionSnapshot snapshot, bool sendSystemNotification)
    {
        InterruptedCount++;
        Publish(new ReminderMessage("interrupted", snapshot.RouteLabel, InfoBarSeverity.Warning));
        return Task.CompletedTask;
    }

    public Task NotifySessionMilestoneAsync(FocusSessionSnapshot snapshot, bool sendSystemNotification)
    {
        MilestoneCount++;
        Publish(new ReminderMessage("milestone", snapshot.RouteLabel, InfoBarSeverity.Informational));
        return Task.CompletedTask;
    }

    public Task NotifySessionStartedAsync(FocusSessionSnapshot snapshot, bool sendSystemNotification)
    {
        StartedCount++;
        Publish(new ReminderMessage("started", snapshot.RouteLabel, InfoBarSeverity.Informational));
        return Task.CompletedTask;
    }

    private void Publish(ReminderMessage message)
    {
        Current = message;
        ReminderRaised?.Invoke(this, message);
    }
}

internal sealed class TestBlockerService : IBlockerService
{
    public event EventHandler<BlockerState>? StateChanged;

    public BlockerState State { get; private set; } = new();

    public int UpdateCallCount { get; private set; }

    public int ReportInterruptionCallCount { get; private set; }

    public Task<bool> CanNavigateAsync(string destinationKey, FocusSessionSnapshot snapshot, AppSettings settings) => Task.FromResult(true);

    public void DismissShield()
    {
        State = State with { IsShieldVisible = false };
        StateChanged?.Invoke(this, State);
    }

    public Task ReportExternalInterruptionAsync(FocusSessionSnapshot snapshot, AppSettings settings)
    {
        ReportInterruptionCallCount++;
        State = State with
        {
            IsShieldVisible = true,
            ShieldHeadline = "interrupt",
            ShieldMessage = snapshot.RouteLabel,
        };
        StateChanged?.Invoke(this, State);
        return Task.CompletedTask;
    }

    public Task UpdateForSessionAsync(FocusSessionSnapshot snapshot, AppSettings settings)
    {
        UpdateCallCount++;
        State = State with
        {
            ShouldStayOnTop = settings.EnableSoftBlocker && settings.KeepWindowOnTop && snapshot.Status == FocusSessionStatus.Running,
            ShouldUseFullScreen = settings.EnableSoftBlocker && settings.UseFocusShield && snapshot.Status == FocusSessionStatus.Running,
            IsShieldVisible = false,
            ShieldHeadline = string.Empty,
            ShieldMessage = string.Empty,
        };
        StateChanged?.Invoke(this, State);
        return Task.CompletedTask;
    }
}

internal sealed class TestTextService : ITextService
{
    private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal)
    {
        ["TagDeepWorkName"] = "Deep Work",
        ["TagStudySprintName"] = "Study Sprint",
        ["TagPlanningName"] = "Planning",
        ["TagCreativeLabName"] = "Creative Lab",
        ["TagAdminSweepName"] = "Admin Sweep",
        ["RouteDeepWorkLabel"] = "Atlantic Focus Corridor",
        ["RouteStudySprintLabel"] = "North Campus Climb",
        ["RoutePlanningLabel"] = "Strategy Holding Pattern",
        ["RouteCreativeLabLabel"] = "Studio Aurora Loop",
        ["RouteAdminSweepLabel"] = "Operations Shuttle",
        ["BadgeRookieTitle"] = "Runway Rookie",
        ["BadgeRookieDescription"] = "Rookie badge",
        ["BadgeCruiserTitle"] = "Cloud Cruiser",
        ["BadgeCruiserDescription"] = "Cruiser badge",
        ["BadgeNavigatorTitle"] = "Route Navigator",
        ["BadgeNavigatorDescription"] = "Navigator badge",
        ["BadgeCaptainTitle"] = "Cabin Captain",
        ["BadgeCaptainDescription"] = "Captain badge",
        ["SoundCabinRainName"] = "Cabin Rain",
        ["SoundCabinRainDescription"] = "Rain",
        ["SoundJetstreamHumName"] = "Jetstream Hum",
        ["SoundJetstreamHumDescription"] = "Hum",
        ["SoundNightCruiseName"] = "Night Cruise",
        ["SoundNightCruiseDescription"] = "Night",
    };

    public string Format(string key, params object?[] arguments)
    {
        string template = GetString(key);
        return string.Format(CultureInfo.InvariantCulture, template, arguments);
    }

    public string GetString(string key)
    {
        return _values.TryGetValue(key, out string? value) ? value : key;
    }
}

internal sealed class TempStoragePathProvider : IStoragePathProvider, IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), "SkyFocusTests", Guid.NewGuid().ToString("N"));

    public string GetLocalDataPath()
    {
        Directory.CreateDirectory(_path);
        return _path;
    }

    public void Dispose()
    {
        if (Directory.Exists(_path))
        {
            Directory.Delete(_path, true);
        }
    }
}