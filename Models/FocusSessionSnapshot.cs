namespace SkyFocus.Models;

public sealed record FocusSessionSnapshot
{
    public static FocusSessionSnapshot Empty { get; } = new();

    public Guid SessionId { get; init; }

    public FocusSessionStatus Status { get; init; } = FocusSessionStatus.Idle;

    public int PlannedMinutes { get; init; } = 25;

    public TimeSpan Elapsed { get; init; } = TimeSpan.Zero;

    public TimeSpan Remaining { get; init; } = TimeSpan.FromMinutes(25);

    public double Progress { get; init; }

    public string TagKey { get; init; } = "deep-work";

    public string TagDisplayName { get; init; } = string.Empty;

    public string RouteLabel { get; init; } = string.Empty;

    public string AmbientSoundId { get; init; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? StartedAtUtc { get; init; }

    public DateTimeOffset? EndsAtUtc { get; init; }

    public bool FiveMinuteReminderSent { get; init; }
}