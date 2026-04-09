using System;
using System.Threading.Tasks;
using SkyFocus.Models;

namespace SkyFocus.Services;

public interface IReminderService
{
    event EventHandler<ReminderMessage>? ReminderRaised;

    ReminderMessage? Current { get; }

    void Dismiss();

    Task NotifySessionStartedAsync(FocusSessionSnapshot snapshot, bool sendSystemNotification);

    Task NotifySessionMilestoneAsync(FocusSessionSnapshot snapshot, bool sendSystemNotification);

    Task NotifySessionCompletedAsync(FocusSessionSnapshot snapshot, bool sendSystemNotification);

    Task NotifySessionInterruptedAsync(FocusSessionSnapshot snapshot, bool sendSystemNotification);
}