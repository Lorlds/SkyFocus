namespace SkyFocus.Models;

public sealed record FocusSessionRecord
{
    public Guid Id { get; init; }

    public string TagKey { get; init; } = string.Empty;

    public string RouteLabel { get; init; } = string.Empty;

    public int PlannedMinutes { get; init; }

    public int ActualMinutes { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset? StartedAtUtc { get; init; }

    public DateTimeOffset EndedAtUtc { get; init; }

    public FocusSessionStatus Status { get; init; }

    public bool UsedFocusShield { get; init; }

    public bool UsedTopMost { get; init; }

    public string AmbientSoundId { get; init; } = string.Empty;
}