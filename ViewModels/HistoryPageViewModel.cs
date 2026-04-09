using System.Collections.ObjectModel;
using SkyFocus.Models;
using SkyFocus.Services;

namespace SkyFocus.ViewModels;

internal sealed class HistoryPageViewModel
{
    private readonly IFocusSessionService _focusSessionService;
    private readonly IStatisticsService _statisticsService;
    private readonly ITextService _textService;

    public HistoryPageViewModel(IFocusSessionService focusSessionService, IStatisticsService statisticsService, ITextService textService)
    {
        _focusSessionService = focusSessionService;
        _statisticsService = statisticsService;
        _textService = textService;
        _focusSessionService.SnapshotChanged += OnSnapshotChanged;
    }

    public ObservableCollection<DailySummaryItem> DailySummaries { get; } = [];

    public ObservableCollection<HistorySessionItem> RecentSessions { get; } = [];

    public async Task InitializeAsync()
    {
        await RefreshAsync().ConfigureAwait(false);
    }

    private async Task RefreshAsync()
    {
        IReadOnlyList<DailySummary> summaries = await _statisticsService.GetDailySummariesAsync(7).ConfigureAwait(false);
        IReadOnlyList<FocusSessionRecord> sessions = await _statisticsService.GetRecentSessionsAsync(12).ConfigureAwait(false);
        Dictionary<string, FocusTag> tags = _focusSessionService.Tags.ToDictionary(tag => tag.Key);

        DailySummaries.Clear();
        foreach (DailySummary summary in summaries.Reverse())
        {
            DailySummaries.Add(new DailySummaryItem(
                summary.Date.ToString("ddd, MMM dd"),
                _textService.Format("HistoryMinutesFormat", summary.MinutesFocused),
                _textService.Format("HistorySessionsFormat", summary.CompletedSessions),
                _textService.Format("HistoryCompletionFormat", (int)Math.Round(summary.CompletionRate * 100)),
                summary.CompletionRate));
        }

        RecentSessions.Clear();
        foreach (FocusSessionRecord session in sessions.Reverse())
        {
            string statusLabel = session.Status switch
            {
                FocusSessionStatus.Completed => _textService.GetString("HistoryStatusCompleted"),
                FocusSessionStatus.Paused => _textService.GetString("HistoryStatusPaused"),
                FocusSessionStatus.Abandoned => _textService.GetString("HistoryStatusAbandoned"),
                _ => _textService.GetString("HistoryStatusIdle"),
            };
            string tagLabel = tags.TryGetValue(session.TagKey, out FocusTag? tag) ? tag.DisplayName : session.TagKey;

            RecentSessions.Add(new HistorySessionItem(
                session.EndedAtUtc.ToLocalTime().ToString("MMM dd, HH:mm"),
                statusLabel,
                _textService.Format("HistoryMinutesFormat", session.ActualMinutes),
                session.RouteLabel,
                tagLabel));
        }
    }

    private void OnSnapshotChanged(object? sender, FocusSessionSnapshot snapshot)
    {
        if (snapshot.Status is FocusSessionStatus.Completed or FocusSessionStatus.Abandoned)
        {
            _ = RefreshAsync();
        }
    }
}