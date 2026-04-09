namespace SkyFocus.Models;

public sealed record DailySummary
{
    public DateOnly Date { get; init; }

    public int MinutesFocused { get; init; }

    public int CompletedSessions { get; init; }

    public double CompletionRate { get; init; }

    public int LongestRunMinutes { get; init; }
}