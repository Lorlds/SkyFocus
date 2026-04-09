using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SkyFocus.Models;
using SkyFocus.Services;

namespace SkyFocus.ViewModels;

internal partial class FocusPageViewModel : ObservableObject
{
    private readonly IAudioService _audioService;
    private readonly IFocusSessionService _focusSessionService;
    private readonly ISettingsService _settingsService;
    private readonly ITextService _textService;
    private bool _initialized;

    public FocusPageViewModel(IAudioService audioService, IFocusSessionService focusSessionService, ISettingsService settingsService, ITextService textService)
    {
        _audioService = audioService;
        _focusSessionService = focusSessionService;
        _settingsService = settingsService;
        _textService = textService;

        _focusSessionService.SnapshotChanged += OnSnapshotChanged;
        _audioService.PlaybackChanged += OnPlaybackChanged;
        DurationPresets = new ObservableCollection<int>([15, 25, 45, 60, 90]);
    }

    public ObservableCollection<int> DurationPresets { get; }

    public ObservableCollection<FocusTag> Tags { get; } = [];

    public ObservableCollection<AmbientSoundProfile> AmbientProfiles { get; } = [];

    [ObservableProperty]
    public partial int SelectedDuration { get; set; } = 25;

    [ObservableProperty]
    public partial FocusTag? SelectedTag { get; set; }

    [ObservableProperty]
    public partial AmbientSoundProfile? SelectedSound { get; set; }

    [ObservableProperty]
    public partial string CountdownText { get; set; } = "25:00";

    [ObservableProperty]
    public partial string RouteLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FlightStateLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ManifestLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProgressText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedSoundDescription { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AmbientPlaybackLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double ProgressValue { get; set; }

    [ObservableProperty]
    public partial bool CanStart { get; set; } = true;

    [ObservableProperty]
    public partial bool CanPause { get; set; }

    [ObservableProperty]
    public partial bool CanResume { get; set; }

    [ObservableProperty]
    public partial bool CanComplete { get; set; }

    [ObservableProperty]
    public partial bool CanAbandon { get; set; }

    public async Task InitializeAsync()
    {
        if (!_initialized)
        {
            AppSettings settings = await _settingsService.GetAsync().ConfigureAwait(false);
            foreach (FocusTag tag in _focusSessionService.Tags)
            {
                Tags.Add(tag);
            }

            foreach (AmbientSoundProfile profile in _audioService.Profiles)
            {
                AmbientProfiles.Add(profile);
            }

            SelectedDuration = settings.DefaultSessionMinutes;
            SelectedTag = Tags.FirstOrDefault(tag => tag.Key == settings.DefaultTagKey) ?? Tags.FirstOrDefault();
            SelectedSound = AmbientProfiles.FirstOrDefault(sound => sound.Id == settings.SelectedAmbientSoundId) ?? AmbientProfiles.FirstOrDefault();
            _initialized = true;
        }

        UpdateFromSnapshot(_focusSessionService.Snapshot);
        UpdateAudioState();
    }

    public async Task AbandonAsync()
    {
        await _focusSessionService.AbandonSessionAsync().ConfigureAwait(false);
        await _audioService.StopAsync().ConfigureAwait(false);
    }

    public async Task CompleteAsync()
    {
        await _focusSessionService.CompleteSessionAsync().ConfigureAwait(false);
        await _audioService.StopAsync().ConfigureAwait(false);
    }

    public async Task PauseAsync()
    {
        await _focusSessionService.PauseSessionAsync().ConfigureAwait(false);
        await _audioService.StopAsync().ConfigureAwait(false);
    }

    public async Task ResumeAsync()
    {
        await _focusSessionService.ResumeSessionAsync().ConfigureAwait(false);
        AppSettings settings = await _settingsService.GetAsync().ConfigureAwait(false);
        if (SelectedSound is not null)
        {
            await _audioService.PlayAsync(SelectedSound.Id, settings.AmbientVolume).ConfigureAwait(false);
        }
    }

    public async Task StartAsync()
    {
        FocusTag tag = SelectedTag ?? Tags.First();
        AmbientSoundProfile? sound = SelectedSound;

        AppSettings settings = await _settingsService.UpdateAsync(current => current with
        {
            DefaultSessionMinutes = SelectedDuration,
            DefaultTagKey = tag.Key,
            SelectedAmbientSoundId = sound?.Id ?? current.SelectedAmbientSoundId,
        }).ConfigureAwait(false);

        await _focusSessionService.CreateSessionAsync(SelectedDuration, tag.Key, sound?.Id ?? settings.SelectedAmbientSoundId).ConfigureAwait(false);
        await _focusSessionService.StartSessionAsync().ConfigureAwait(false);

        if (sound is not null)
        {
            await _audioService.PlayAsync(sound.Id, settings.AmbientVolume).ConfigureAwait(false);
        }
    }

    partial void OnSelectedSoundChanged(AmbientSoundProfile? value)
    {
        SelectedSoundDescription = value?.Description ?? _textService.GetString("FocusNoSoundDescription");
    }

    private void OnPlaybackChanged(object? sender, EventArgs eventArgs)
    {
        UpdateAudioState();
    }

    private void OnSnapshotChanged(object? sender, FocusSessionSnapshot snapshot)
    {
        UpdateFromSnapshot(snapshot);
    }

    private void UpdateAudioState()
    {
        AmbientPlaybackLabel = _audioService.IsPlaying
            ? _textService.Format("FocusAmbientPlayingFormat", SelectedSound?.DisplayName ?? _textService.GetString("FocusNoSoundLabel"))
            : _textService.GetString("FocusAmbientStoppedLabel");
    }

    private void UpdateFromSnapshot(FocusSessionSnapshot snapshot)
    {
        if (snapshot.Status == FocusSessionStatus.Idle)
        {
            RouteLabel = SelectedTag?.RouteLabel ?? _textService.GetString("FocusReadyRoute");
            CountdownText = TimeSpan.FromMinutes(SelectedDuration).ToString(@"mm\:ss");
            FlightStateLabel = _textService.GetString("FocusStateIdle");
            ManifestLabel = _textService.Format("FocusManifestFormat", SelectedDuration, SelectedTag?.DisplayName ?? _textService.GetString("TagDeepWorkName"));
            ProgressText = _textService.GetString("FocusProgressIdle");
            ProgressValue = 0;
            CanStart = true;
            CanPause = false;
            CanResume = false;
            CanComplete = false;
            CanAbandon = false;
            return;
        }

        RouteLabel = snapshot.RouteLabel;
        CountdownText = snapshot.Remaining.TotalHours >= 1 ? snapshot.Remaining.ToString(@"hh\:mm\:ss") : snapshot.Remaining.ToString(@"mm\:ss");
        ManifestLabel = _textService.Format("FocusManifestFormat", snapshot.PlannedMinutes, snapshot.TagDisplayName);
        ProgressText = _textService.Format("FocusProgressFormat", (int)Math.Round(snapshot.Progress * 100), snapshot.TagDisplayName);
        ProgressValue = snapshot.Progress;

        switch (snapshot.Status)
        {
            case FocusSessionStatus.Running:
                FlightStateLabel = _textService.GetString("FocusStateRunning");
                CanStart = false;
                CanPause = true;
                CanResume = false;
                CanComplete = true;
                CanAbandon = true;
                break;
            case FocusSessionStatus.Paused:
                FlightStateLabel = _textService.GetString("FocusStatePaused");
                CanStart = false;
                CanPause = false;
                CanResume = true;
                CanComplete = true;
                CanAbandon = true;
                break;
            case FocusSessionStatus.Completed:
                FlightStateLabel = _textService.GetString("FocusStateCompleted");
                CanStart = true;
                CanPause = false;
                CanResume = false;
                CanComplete = false;
                CanAbandon = false;
                break;
            default:
                FlightStateLabel = _textService.GetString("FocusStateAbandoned");
                CanStart = true;
                CanPause = false;
                CanResume = false;
                CanComplete = false;
                CanAbandon = false;
                break;
        }
    }
}
