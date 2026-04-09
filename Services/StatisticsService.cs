using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkyFocus.Models;

namespace SkyFocus.Services;

internal sealed class StatisticsService : IStatisticsService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ITextService _textService;

    public StatisticsService(ISessionRepository sessionRepository, ITextService textService)
    {
        _sessionRepository = sessionRepository;
        _textService = textService;
    }

    public async Task<AchievementState> GetAchievementStateAsync()
    {
        IReadOnlyList<FocusSessionRecord> sessions = await _sessionRepository.GetSessionsAsync().ConfigureAwait(false);
        List<DateOnly> completedDates = sessions
            .Where(session => session.Status == FocusSessionStatus.Completed)
            .Select(session => DateOnly.FromDateTime(session.EndedAtUtc.ToLocalTime().DateTime))
            .Distinct()
            .OrderBy(date => date)
            .ToList();

        int currentStreak = CalculateCurrentStreak(completedDates);
        int longestStreak = CalculateLongestStreak(completedDates);
        int completedToday = sessions.Count(session => session.Status == FocusSessionStatus.Completed && DateOnly.FromDateTime(session.EndedAtUtc.ToLocalTime().DateTime) == DateOnly.FromDateTime(DateTime.Now));

        (string badgeTitle, string badgeDescription) = longestStreak switch
        {
            >= 21 => (_textService.GetString("BadgeCaptainTitle"), _textService.GetString("BadgeCaptainDescription")),
            >= 7 => (_textService.GetString("BadgeNavigatorTitle"), _textService.GetString("BadgeNavigatorDescription")),
            >= 3 => (_textService.GetString("BadgeCruiserTitle"), _textService.GetString("BadgeCruiserDescription")),
            _ => (_textService.GetString("BadgeRookieTitle"), _textService.GetString("BadgeRookieDescription"))
        };

        return new AchievementState
        {
            CurrentStreakDays = currentStreak,
            LongestStreakDays = longestStreak,
            CompletedToday = completedToday,
            BadgeTitle = badgeTitle,
            BadgeDescription = badgeDescription,
        };
    }

    public async Task<IReadOnlyList<DailySummary>> GetDailySummariesAsync(int days)
    {
        DateOnly startDate = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(-(days - 1)));
        DateTimeOffset localStart = new(startDate.ToDateTime(TimeOnly.MinValue), TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));
        IReadOnlyList<FocusSessionRecord> sessions = await _sessionRepository.GetSessionsAsync(localStart.ToUniversalTime()).ConfigureAwait(false);
        Dictionary<DateOnly, List<FocusSessionRecord>> grouped = sessions.GroupBy(session => DateOnly.FromDateTime(session.EndedAtUtc.ToLocalTime().DateTime)).ToDictionary(group => group.Key, group => group.ToList());

        List<DailySummary> summaries = [];
        for (int offset = 0; offset < days; offset++)
        {
            DateOnly date = startDate.AddDays(offset);
            grouped.TryGetValue(date, out List<FocusSessionRecord>? daySessions);
            daySessions ??= [];

            int completedMinutes = daySessions.Where(session => session.Status == FocusSessionStatus.Completed).Sum(session => session.ActualMinutes);
            int completedSessions = daySessions.Count(session => session.Status == FocusSessionStatus.Completed);
            int totalSessions = daySessions.Count;
            int longestRun = daySessions.Count == 0 ? 0 : daySessions.Max(session => session.ActualMinutes);

            summaries.Add(new DailySummary
            {
                Date = date,
                MinutesFocused = completedMinutes,
                CompletedSessions = completedSessions,
                CompletionRate = totalSessions == 0 ? 0 : (double)completedSessions / totalSessions,
                LongestRunMinutes = longestRun,
            });
        }

        return summaries;
    }

    public async Task<IReadOnlyList<FocusSessionRecord>> GetRecentSessionsAsync(int count)
    {
        return await _sessionRepository.GetRecentSessionsAsync(count).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TagBreakdown>> GetTagBreakdownAsync(int days, IReadOnlyList<FocusTag> tags)
    {
        DateTimeOffset localStart = new(DateTime.Now.Date.AddDays(-(days - 1)), TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));
        IReadOnlyList<FocusSessionRecord> sessions = await _sessionRepository.GetSessionsAsync(localStart.ToUniversalTime()).ConfigureAwait(false);
        Dictionary<string, FocusTag> tagLookup = tags.ToDictionary(tag => tag.Key);

        return sessions
            .Where(session => session.Status == FocusSessionStatus.Completed)
            .GroupBy(session => session.TagKey)
            .Select(group =>
            {
                FocusTag tag = tagLookup.TryGetValue(group.Key, out FocusTag? foundTag) ? foundTag : new FocusTag(group.Key, group.Key, string.Empty, group.Key);
                return new TagBreakdown(group.Key, tag.DisplayName, tag.Glyph, group.Sum(session => session.ActualMinutes));
            })
            .OrderByDescending(item => item.MinutesFocused)
            .ToList();
    }

    public async Task<int> GetTodayFocusMinutesAsync()
    {
        DateTimeOffset localStart = new(DateTime.Now.Date, TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));
        IReadOnlyList<FocusSessionRecord> sessions = await _sessionRepository.GetSessionsAsync(localStart.ToUniversalTime()).ConfigureAwait(false);
        return sessions.Where(session => session.Status == FocusSessionStatus.Completed).Sum(session => session.ActualMinutes);
    }

    private static int CalculateCurrentStreak(IReadOnlyList<DateOnly> completedDates)
    {
        if (completedDates.Count == 0)
        {
            return 0;
        }

        HashSet<DateOnly> completedLookup = [.. completedDates];
        DateOnly cursor = DateOnly.FromDateTime(DateTime.Now);
        int streak = 0;

        while (completedLookup.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }

    private static int CalculateLongestStreak(IReadOnlyList<DateOnly> completedDates)
    {
        if (completedDates.Count == 0)
        {
            return 0;
        }

        int longest = 1;
        int current = 1;
        for (int index = 1; index < completedDates.Count; index++)
        {
            if (completedDates[index] == completedDates[index - 1].AddDays(1))
            {
                current++;
            }
            else
            {
                current = 1;
            }

            longest = Math.Max(longest, current);
        }

        return longest;
    }
}