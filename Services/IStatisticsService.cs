using System.Collections.Generic;
using System.Threading.Tasks;
using SkyFocus.Models;

namespace SkyFocus.Services;

public interface IStatisticsService
{
    Task<IReadOnlyList<FocusSessionRecord>> GetRecentSessionsAsync(int count);

    Task<IReadOnlyList<DailySummary>> GetDailySummariesAsync(int days);

    Task<IReadOnlyList<TagBreakdown>> GetTagBreakdownAsync(int days, IReadOnlyList<FocusTag> tags);

    Task<AchievementState> GetAchievementStateAsync();

    Task<int> GetTodayFocusMinutesAsync();
}