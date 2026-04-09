namespace SkyFocus.Models;

public sealed record AppSettings
{
    public int DefaultSessionMinutes { get; init; } = 25;

    public string DefaultTagKey { get; init; } = "deep-work";

    public string Theme { get; init; } = "System";

    public bool EnableSoftBlocker { get; init; } = true;

    public bool UseFocusShield { get; init; } = true;

    public bool KeepWindowOnTop { get; init; }

    public bool SendSystemNotifications { get; init; } = true;

    public bool NotifyAtFiveMinutes { get; init; } = true;

    public double AmbientVolume { get; init; } = 0.55;

    public string SelectedAmbientSoundId { get; init; } = "cabin-rain";
}