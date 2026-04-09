using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using SkyFocus.Models;

namespace SkyFocus.Services;

internal sealed class AudioService : IAudioService, IDisposable
{
    private readonly MediaPlayer _player = new();
    private readonly IReadOnlyList<AmbientSoundProfile> _profiles;

    public AudioService(ITextService textService)
    {
        _profiles = CatalogFactory.CreateSounds(textService);
        _player.IsLoopingEnabled = true;
        _player.Volume = 0.55;
    }

    public event EventHandler? PlaybackChanged;

    public IReadOnlyList<AmbientSoundProfile> Profiles => _profiles;

    public string? ActiveProfileId { get; private set; }

    public bool IsPlaying { get; private set; }

    public double Volume => _player.Volume;

    public Task PlayAsync(string profileId, double volume)
    {
        AmbientSoundProfile? profile = _profiles.FirstOrDefault(item => string.Equals(item.Id, profileId, StringComparison.Ordinal));
        if (profile is null)
        {
            return Task.CompletedTask;
        }

        _player.Source = MediaSource.CreateFromUri(new Uri(profile.AssetUri));
        _player.Volume = Math.Clamp(volume, 0.0, 1.0);
        _player.Play();

        ActiveProfileId = profile.Id;
        IsPlaying = true;
        PlaybackChanged?.Invoke(this, EventArgs.Empty);

        return Task.CompletedTask;
    }

    public Task SetVolumeAsync(double volume)
    {
        _player.Volume = Math.Clamp(volume, 0.0, 1.0);
        PlaybackChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _player.Pause();
        IsPlaying = false;
        ActiveProfileId = null;
        PlaybackChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _player.Dispose();
    }
}