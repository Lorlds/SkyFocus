namespace SkyFocus.Models;

public sealed record BlockerState
{
    public bool ShouldUseFullScreen { get; init; }

    public bool ShouldStayOnTop { get; init; }

    public bool IsShieldVisible { get; init; }

    public string ShieldHeadline { get; init; } = string.Empty;

    public string ShieldMessage { get; init; } = string.Empty;
}