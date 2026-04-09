namespace SkyFocus.Models;

public sealed record FocusTag(
    string Key,
    string DisplayName,
    string Glyph,
    string RouteLabel);