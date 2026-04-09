using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SkyFocus.Models;
using SkyFocus.Services;

namespace SkyFocus.ViewModels;

internal partial class HomePageViewModel : ObservableObject
{
    private readonly IFocusSessionService _focusSessionService;
    private readonly IStatisticsService _statisticsService;
    private readonly ITextService _textService;

    public HomePageViewModel(IFocusSessionService focusSessionService, IStatisticsService statisticsService, ITextService textService)
    {
        _focusSessionService = focusSessionService;
        _statisticsService = statisticsService;
        _textService = textService;
        _focusSessionService.SnapshotChanged += OnSnapshotChanged;
    }

    public ObservableCollection<TagBreakdown> TopTags { get; } = [];

    [ObservableProperty]
    public partial string JourneyTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string JourneySubtitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TodayMinutesText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TodaySessionsText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StreakText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BadgeTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BadgeDescription { get; set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await RefreshAsync().ConfigureAwait(false);
        UpdateJourney(_focusSessionService.Snapshot);
    }

    private async Task RefreshAsync()
    {
        IReadOnlyList<DailySummary> summaries = await _statisticsService.GetDailySummariesAsync(1).ConfigureAwait(false);
        DailySummary today = summaries[0];
        AchievementState achievement = await _statisticsService.GetAchievementStateAsync().ConfigureAwait(false);
        IReadOnlyList<TagBreakdown> tagBreakdowns = await _statisticsService.GetTagBreakdownAsync(7, _focusSessionService.Tags).ConfigureAwait(false);

        TodayMinutesText = _textService.Format("HomeTodayMinutesFormat", today.MinutesFocused);
        TodaySessionsText = _textService.Format("HomeTodaySessionsFormat", today.CompletedSessions);
        StreakText = _textService.Format("HomeStreakFormat", achievement.CurrentStreakDays);
        BadgeTitle = achievement.BadgeTitle;
        BadgeDescription = achievement.BadgeDescription;

        TopTags.Clear();
        foreach (TagBreakdown tag in tagBreakdowns.Take(3))
        {
            TopTags.Add(tag);
        }
    }

    private void OnSnapshotChanged(object? sender, FocusSessionSnapshot snapshot)
    {
        UpdateJourney(snapshot);
        if (snapshot.Status is FocusSessionStatus.Completed or FocusSessionStatus.Abandoned)
        {
            _ = RefreshAsync();
        }
    }

    private void UpdateJourney(FocusSessionSnapshot snapshot)
    {
        if (snapshot.Status == FocusSessionStatus.Running)
        {
            JourneyTitle = snapshot.RouteLabel;
            JourneySubtitle = _textService.Format("HomeJourneyRunningFormat", Math.Max(1, (int)Math.Ceiling(snapshot.Remaining.TotalMinutes)), snapshot.TagDisplayName);
            return;
        }

        if (snapshot.Status == FocusSessionStatus.Paused)
        {
            JourneyTitle = snapshot.RouteLabel;
            JourneySubtitle = _textService.GetString("HomeJourneyPaused");
            return;
        }

        JourneyTitle = _textService.GetString("HomeReadyTitle");
        JourneySubtitle = _textService.GetString("HomeReadySubtitle");
    }
}