using System;
using System.Threading.Tasks;
using SkyFocus.Models;

namespace SkyFocus.Services;

internal sealed class BlockerService : IBlockerService
{
    private readonly ITextService _textService;

    public BlockerService(ITextService textService)
    {
        _textService = textService;
    }

    public event EventHandler<BlockerState>? StateChanged;

    public BlockerState State { get; private set; } = new();

    public Task<bool> CanNavigateAsync(string destinationKey, FocusSessionSnapshot snapshot, AppSettings settings)
    {
        if (!settings.EnableSoftBlocker || snapshot.Status is not FocusSessionStatus.Running and not FocusSessionStatus.Paused)
        {
            return Task.FromResult(true);
        }

        if (string.Equals(destinationKey, "focus", StringComparison.OrdinalIgnoreCase) || string.Equals(destinationKey, "home", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(true);
        }

        Publish(State with
        {
            IsShieldVisible = true,
            ShieldHeadline = _textService.GetString("ShieldNavigationHeadline"),
            ShieldMessage = _textService.Format("ShieldNavigationMessageFormat", snapshot.RouteLabel)
        });

        return Task.FromResult(false);
    }

    public void DismissShield()
    {
        if (!State.IsShieldVisible)
        {
            return;
        }

        Publish(State with
        {
            IsShieldVisible = false,
            ShieldHeadline = string.Empty,
            ShieldMessage = string.Empty,
        });
    }

    public Task ReportExternalInterruptionAsync(FocusSessionSnapshot snapshot, AppSettings settings)
    {
        if (!settings.EnableSoftBlocker || snapshot.Status != FocusSessionStatus.Running)
        {
            return Task.CompletedTask;
        }

        Publish(State with
        {
            IsShieldVisible = true,
            ShieldHeadline = _textService.GetString("ShieldReturnHeadline"),
            ShieldMessage = _textService.Format("ShieldReturnMessageFormat", snapshot.RouteLabel),
        });

        return Task.CompletedTask;
    }

    public Task UpdateForSessionAsync(FocusSessionSnapshot snapshot, AppSettings settings)
    {
        bool shouldPin = settings.EnableSoftBlocker && settings.KeepWindowOnTop && snapshot.Status == FocusSessionStatus.Running;
        bool shouldFullscreen = settings.EnableSoftBlocker && settings.UseFocusShield && snapshot.Status == FocusSessionStatus.Running;

        Publish(State with
        {
            ShouldStayOnTop = shouldPin,
            ShouldUseFullScreen = shouldFullscreen,
            IsShieldVisible = State.IsShieldVisible && snapshot.Status == FocusSessionStatus.Running,
        });

        return Task.CompletedTask;
    }

    private void Publish(BlockerState state)
    {
        if (State == state)
        {
            return;
        }

        State = state;
        StateChanged?.Invoke(this, State);
    }
}