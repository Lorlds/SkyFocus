namespace SkyFocus.Models;

public sealed record TagBreakdown(
    string Key,
    string DisplayName,
    string Glyph,
    int MinutesFocused);