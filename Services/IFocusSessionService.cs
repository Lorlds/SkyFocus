using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkyFocus.Models;

namespace SkyFocus.Services;

public interface IFocusSessionService
{
    event EventHandler<FocusSessionSnapshot>? SnapshotChanged;

    FocusSessionSnapshot Snapshot { get; }

    IReadOnlyList<FocusTag> Tags { get; }

    Task<FocusSessionSnapshot> CreateSessionAsync(int plannedMinutes, string tagKey, string ambientSoundId);

    Task StartSessionAsync();

    Task PauseSessionAsync();

    Task ResumeSessionAsync();

    Task CompleteSessionAsync();

    Task AbandonSessionAsync();
}