using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using SkyFocus.Models;

namespace SkyFocus.Services;

internal sealed class ReminderService : IReminderService
{
    private readonly ITextService _textService;
    private bool _notificationRegistrationAttempted;

    public ReminderService(ITextService textService)
    {
        _textService = textService;
    }

    public event EventHandler<ReminderMessage>? ReminderRaised;

    public ReminderMessage? Current { get; private set; }

    public void Dismiss()
    {
        Current = null;
    }

    public Task NotifySessionCompletedAsync(FocusSessionSnapshot snapshot, bool sendSystemNotification)
    {
        string title = _textService.GetString("ReminderCompletedTitle");
        string body = _textService.Format("ReminderCompletedBodyFormat", snapshot.RouteLabel);
        return PublishAsync(title, body, InfoBarSeverity.Success, sendSystemNotification);
    }

    public Task NotifySessionInterruptedAsync(FocusSessionSnapshot snapshot, bool sendSystemNotification)
    {
        string title = _textService.GetString("ReminderInterruptedTitle");
        string body = _textService.Format("ReminderInterruptedBodyFormat", snapshot.RouteLabel);
        return PublishAsync(title, body, InfoBarSeverity.Warning, sendSystemNotification);
    }

    public Task NotifySessionMilestoneAsync(FocusSessionSnapshot snapshot, bool sendSystemNotification)
    {
        string title = _textService.GetString("ReminderMilestoneTitle");
        string body = _textService.Format("ReminderMilestoneBodyFormat", Math.Max(1, (int)Math.Ceiling(snapshot.Remaining.TotalMinutes)), snapshot.RouteLabel);
        return PublishAsync(title, body, InfoBarSeverity.Informational, sendSystemNotification);
    }

    public Task NotifySessionStartedAsync(FocusSessionSnapshot snapshot, bool sendSystemNotification)
    {
        string title = _textService.GetString("ReminderStartedTitle");
        string body = _textService.Format("ReminderStartedBodyFormat", snapshot.PlannedMinutes.ToString(CultureInfo.CurrentCulture), snapshot.RouteLabel);
        return PublishAsync(title, body, InfoBarSeverity.Informational, sendSystemNotification);
    }

    private Task PublishAsync(string title, string body, InfoBarSeverity severity, bool sendSystemNotification)
    {
        Current = new ReminderMessage(title, body, severity);
        ReminderRaised?.Invoke(this, Current);

        if (sendSystemNotification)
        {
            TryShowSystemNotification(title, body);
        }

        return Task.CompletedTask;
    }

    private void TryShowSystemNotification(string title, string body)
    {
        try
        {
            if (!_notificationRegistrationAttempted)
            {
                AppNotificationManager.Default.Register();
                _notificationRegistrationAttempted = true;
            }

            AppNotification notification = new AppNotificationBuilder().AddText(title).AddText(body).BuildNotification();
            AppNotificationManager.Default.Show(notification);
        }
        catch
        {
        }
    }
}