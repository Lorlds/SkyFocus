using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SkyFocus.Models;

namespace SkyFocus.Services;

internal sealed class FocusSessionService : IFocusSessionService
{
    private readonly IBlockerService _blockerService;
    private readonly IReminderService _reminderService;
    private readonly ISessionRepository _sessionRepository;
    private readonly ISettingsService _settingsService;
    private readonly IReadOnlyList<FocusTag> _tags;
    private CancellationTokenSource? _timerTokenSource;

    public FocusSessionService(ISessionRepository sessionRepository, ISettingsService settingsService, IReminderService reminderService, IBlockerService blockerService, ITextService textService)
    {
        _sessionRepository = sessionRepository;
        _settingsService = settingsService;
        _reminderService = reminderService;
        _blockerService = blockerService;
        _tags = CatalogFactory.CreateTags(textService);
    }

    public event EventHandler<FocusSessionSnapshot>? SnapshotChanged;

    public FocusSessionSnapshot Snapshot { get; private set; } = FocusSessionSnapshot.Empty;

    public IReadOnlyList<FocusTag> Tags => _tags;

    public async Task<FocusSessionSnapshot> CreateSessionAsync(int plannedMinutes, string tagKey, string ambientSoundId)
    {
        if (Snapshot.Status is FocusSessionStatus.Running or FocusSessionStatus.Paused)
        {
            throw new InvalidOperationException("A session is already in progress.");
        }

        FocusTag selectedTag = _tags.FirstOrDefault(tag => string.Equals(tag.Key, tagKey, StringComparison.Ordinal)) ?? _tags[0];
        int clampedMinutes = Math.Clamp(plannedMinutes, 10, 180);

        Snapshot = new FocusSessionSnapshot
        {
            SessionId = Guid.NewGuid(),
            Status = FocusSessionStatus.Idle,
            PlannedMinutes = clampedMinutes,
            Remaining = TimeSpan.FromMinutes(clampedMinutes),
            TagKey = selectedTag.Key,
            TagDisplayName = selectedTag.DisplayName,
            RouteLabel = selectedTag.RouteLabel,
            AmbientSoundId = ambientSoundId,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        await UpdateBlockerAsync().ConfigureAwait(false);
        PublishSnapshot();
        return Snapshot;
    }

    public async Task StartSessionAsync()
    {
        if (Snapshot.Status != FocusSessionStatus.Idle)
        {
            return;
        }

        DateTimeOffset startedAt = DateTimeOffset.UtcNow;
        Snapshot = Snapshot with
        {
            Status = FocusSessionStatus.Running,
            StartedAtUtc = startedAt,
            EndsAtUtc = startedAt.AddMinutes(Snapshot.PlannedMinutes),
            Elapsed = TimeSpan.Zero,
            Remaining = TimeSpan.FromMinutes(Snapshot.PlannedMinutes),
            Progress = 0,
            FiveMinuteReminderSent = false,
        };

        await UpdateBlockerAsync().ConfigureAwait(false);
        AppSettings settings = await _settingsService.GetAsync().ConfigureAwait(false);
        await _reminderService.NotifySessionStartedAsync(Snapshot, settings.SendSystemNotifications).ConfigureAwait(false);
        PublishSnapshot();
        StartTimerLoop();
    }

    public async Task PauseSessionAsync()
    {
        if (Snapshot.Status != FocusSessionStatus.Running)
        {
            return;
        }

        StopTimerLoop();
        Snapshot = BuildLiveSnapshot(DateTimeOffset.UtcNow) with
        {
            Status = FocusSessionStatus.Paused,
            EndsAtUtc = null,
        };

        await UpdateBlockerAsync().ConfigureAwait(false);
        PublishSnapshot();
    }

    public async Task ResumeSessionAsync()
    {
        if (Snapshot.Status != FocusSessionStatus.Paused)
        {
            return;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        Snapshot = Snapshot with
        {
            Status = FocusSessionStatus.Running,
            StartedAtUtc = now - Snapshot.Elapsed,
            EndsAtUtc = now + Snapshot.Remaining,
        };

        await UpdateBlockerAsync().ConfigureAwait(false);
        PublishSnapshot();
        StartTimerLoop();
    }

    public Task CompleteSessionAsync()
    {
        return EndSessionAsync(FocusSessionStatus.Completed);
    }

    public Task AbandonSessionAsync()
    {
        return EndSessionAsync(FocusSessionStatus.Abandoned);
    }

    private FocusSessionSnapshot BuildLiveSnapshot(DateTimeOffset now)
    {
        if (Snapshot.StartedAtUtc is null)
        {
            return Snapshot;
        }

        TimeSpan elapsed = now - Snapshot.StartedAtUtc.Value;
        TimeSpan planned = TimeSpan.FromMinutes(Snapshot.PlannedMinutes);
        TimeSpan remaining = planned - elapsed;
        if (remaining < TimeSpan.Zero)
        {
            remaining = TimeSpan.Zero;
        }

        double progress = planned.TotalSeconds <= 0 ? 0 : Math.Clamp(elapsed.TotalSeconds / planned.TotalSeconds, 0, 1);
        return Snapshot with
        {
            Elapsed = elapsed,
            Remaining = remaining,
            Progress = progress,
            EndsAtUtc = Snapshot.StartedAtUtc.Value + planned,
        };
    }

    private async Task EndSessionAsync(FocusSessionStatus finalStatus)
    {
        if (Snapshot.Status is not FocusSessionStatus.Running and not FocusSessionStatus.Paused and not FocusSessionStatus.Idle)
        {
            return;
        }

        StopTimerLoop();
        FocusSessionSnapshot finalSnapshot = Snapshot.Status == FocusSessionStatus.Running ? BuildLiveSnapshot(DateTimeOffset.UtcNow) : Snapshot;
        finalSnapshot = finalSnapshot with
        {
            Status = finalStatus,
            Remaining = finalStatus == FocusSessionStatus.Completed ? TimeSpan.Zero : finalSnapshot.Remaining,
            Progress = finalStatus == FocusSessionStatus.Completed ? 1 : finalSnapshot.Progress,
            EndsAtUtc = DateTimeOffset.UtcNow,
        };

        Snapshot = finalSnapshot;
        await PersistSessionAsync(finalSnapshot).ConfigureAwait(false);
        await UpdateBlockerAsync().ConfigureAwait(false);

        AppSettings settings = await _settingsService.GetAsync().ConfigureAwait(false);
        if (finalStatus == FocusSessionStatus.Completed)
        {
            await _reminderService.NotifySessionCompletedAsync(finalSnapshot, settings.SendSystemNotifications).ConfigureAwait(false);
        }

        PublishSnapshot();
    }

    private async Task PersistSessionAsync(FocusSessionSnapshot finalSnapshot)
    {
        AppSettings settings = await _settingsService.GetAsync().ConfigureAwait(false);
        FocusSessionRecord record = new()
        {
            Id = finalSnapshot.SessionId,
            TagKey = finalSnapshot.TagKey,
            RouteLabel = finalSnapshot.RouteLabel,
            PlannedMinutes = finalSnapshot.PlannedMinutes,
            ActualMinutes = Math.Max(1, (int)Math.Ceiling(finalSnapshot.Elapsed.TotalMinutes)),
            CreatedAtUtc = finalSnapshot.CreatedAtUtc,
            StartedAtUtc = finalSnapshot.StartedAtUtc,
            EndedAtUtc = DateTimeOffset.UtcNow,
            Status = finalSnapshot.Status,
            UsedFocusShield = settings.UseFocusShield,
            UsedTopMost = settings.KeepWindowOnTop,
            AmbientSoundId = finalSnapshot.AmbientSoundId,
        };

        await _sessionRepository.InitializeAsync().ConfigureAwait(false);
        await _sessionRepository.AddSessionAsync(record).ConfigureAwait(false);
    }

    private void PublishSnapshot()
    {
        SnapshotChanged?.Invoke(this, Snapshot);
    }

    private void StartTimerLoop()
    {
        StopTimerLoop();
        _timerTokenSource = new CancellationTokenSource();
        _ = RunTimerLoopAsync(_timerTokenSource.Token);
    }

    private void StopTimerLoop()
    {
        if (_timerTokenSource is null)
        {
            return;
        }

        _timerTokenSource.Cancel();
        _timerTokenSource.Dispose();
        _timerTokenSource = null;
    }

    private async Task RunTimerLoopAsync(CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));

        while (!cancellationToken.IsCancellationRequested && await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
        {
            if (Snapshot.Status != FocusSessionStatus.Running)
            {
                return;
            }

            Snapshot = BuildLiveSnapshot(DateTimeOffset.UtcNow);
            PublishSnapshot();

            if (!Snapshot.FiveMinuteReminderSent && Snapshot.Remaining <= TimeSpan.FromMinutes(5) && Snapshot.Remaining > TimeSpan.Zero)
            {
                AppSettings settings = await _settingsService.GetAsync().ConfigureAwait(false);
                Snapshot = Snapshot with { FiveMinuteReminderSent = true };
                await _reminderService.NotifySessionMilestoneAsync(Snapshot, settings.SendSystemNotifications && settings.NotifyAtFiveMinutes).ConfigureAwait(false);
                PublishSnapshot();
            }

            if (Snapshot.Remaining == TimeSpan.Zero)
            {
                await CompleteSessionAsync().ConfigureAwait(false);
                return;
            }
        }
    }

    private async Task UpdateBlockerAsync()
    {
        AppSettings settings = await _settingsService.GetAsync().ConfigureAwait(false);
        await _blockerService.UpdateForSessionAsync(Snapshot, settings).ConfigureAwait(false);
    }
}