using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SkyFocus.Models;
using SkyFocus.Services;

namespace SkyFocus.ViewModels;

internal partial class SoundsPageViewModel : ObservableObject
{
    private readonly IAudioService _audioService;
    private readonly ISettingsService _settingsService;
    private readonly ITextService _textService;
    private bool _initialized;

    public SoundsPageViewModel(IAudioService audioService, ISettingsService settingsService, ITextService textService)
    {
        _audioService = audioService;
        _settingsService = settingsService;
        _textService = textService;
        _audioService.PlaybackChanged += OnPlaybackChanged;
    }

    public ObservableCollection<AmbientSoundProfile> Profiles { get; } = [];

    [ObservableProperty]
    public partial AmbientSoundProfile? SelectedProfile { get; set; }

    [ObservableProperty]
    public partial double Volume { get; set; } = 0.55;

    [ObservableProperty]
    public partial bool IsPlaying { get; set; }

    [ObservableProperty]
    public partial string PlaybackHeadline { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedProfileDescription { get; set; } = string.Empty;

    public async Task InitializeAsync()
    {
        if (!_initialized)
        {
            foreach (AmbientSoundProfile profile in _audioService.Profiles)
            {
                Profiles.Add(profile);
            }

            AppSettings settings = await _settingsService.GetAsync().ConfigureAwait(false);
            SelectedProfile = Profiles.FirstOrDefault(profile => profile.Id == settings.SelectedAmbientSoundId) ?? Profiles.FirstOrDefault();
            Volume = settings.AmbientVolume;
            _initialized = true;
        }

        UpdatePlaybackState();
    }

    public async Task PlaySelectedAsync()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        await _audioService.PlayAsync(SelectedProfile.Id, Volume).ConfigureAwait(false);
        await SaveSelectionAsync().ConfigureAwait(false);
        UpdatePlaybackState();
    }

    public async Task SaveSelectionAsync()
    {
        await _settingsService.UpdateAsync(settings => settings with
        {
            AmbientVolume = Volume,
            SelectedAmbientSoundId = SelectedProfile?.Id ?? settings.SelectedAmbientSoundId,
        }).ConfigureAwait(false);
    }

    public async Task StopAsync()
    {
        await _audioService.StopAsync().ConfigureAwait(false);
        UpdatePlaybackState();
    }

    public async Task UpdateVolumeAsync()
    {
        await _audioService.SetVolumeAsync(Volume).ConfigureAwait(false);
        await SaveSelectionAsync().ConfigureAwait(false);
        UpdatePlaybackState();
    }

    partial void OnSelectedProfileChanged(AmbientSoundProfile? value)
    {
        SelectedProfileDescription = value?.Description ?? string.Empty;
    }

    private void OnPlaybackChanged(object? sender, EventArgs eventArgs)
    {
        UpdatePlaybackState();
    }

    private void UpdatePlaybackState()
    {
        IsPlaying = _audioService.IsPlaying;
        PlaybackHeadline = _audioService.IsPlaying && SelectedProfile is not null
            ? _textService.Format("SoundsPlayingFormat", SelectedProfile.DisplayName)
            : _textService.GetString("SoundsStoppedLabel");
    }
}
