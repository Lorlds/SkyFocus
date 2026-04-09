using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkyFocus.Models;

namespace SkyFocus.Services;

public interface IAudioService
{
    event EventHandler? PlaybackChanged;

    IReadOnlyList<AmbientSoundProfile> Profiles { get; }

    string? ActiveProfileId { get; }

    bool IsPlaying { get; }

    double Volume { get; }

    Task PlayAsync(string profileId, double volume);

    Task SetVolumeAsync(double volume);

    Task StopAsync();
}