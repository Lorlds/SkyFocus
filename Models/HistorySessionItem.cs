namespace SkyFocus.Models;

public sealed record HistorySessionItem(
    string DateLabel,
    string StatusLabel,
    string DurationLabel,
    string RouteLabel,
    string TagLabel);