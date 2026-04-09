namespace SkyFocus.Models;

public sealed record DailySummaryItem(
    string DateLabel,
    string MinutesLabel,
    string SessionsLabel,
    string CompletionLabel,
    double CompletionValue);