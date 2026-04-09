using System;
using System.Threading.Tasks;
using SkyFocus.Models;

namespace SkyFocus.Services;

public interface IBlockerService
{
    event EventHandler<BlockerState>? StateChanged;

    BlockerState State { get; }

    Task UpdateForSessionAsync(FocusSessionSnapshot snapshot, AppSettings settings);

    Task<bool> CanNavigateAsync(string destinationKey, FocusSessionSnapshot snapshot, AppSettings settings);

    Task ReportExternalInterruptionAsync(FocusSessionSnapshot snapshot, AppSettings settings);

    void DismissShield();
}