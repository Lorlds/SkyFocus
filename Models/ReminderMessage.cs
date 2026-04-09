using Microsoft.UI.Xaml.Controls;

namespace SkyFocus.Models;

public sealed record ReminderMessage(
    string Title,
    string Body,
    InfoBarSeverity Severity);