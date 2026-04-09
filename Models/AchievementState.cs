namespace SkyFocus.Models;

public sealed record AchievementState
{
    public int CurrentStreakDays { get; init; }

    public int LongestStreakDays { get; init; }

    public int CompletedToday { get; init; }

    public string BadgeTitle { get; init; } = string.Empty;

    public string BadgeDescription { get; init; } = string.Empty;
}