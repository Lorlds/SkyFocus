using SkyFocus.Models;
using SkyFocus.Services;
using SkyFocus.Tests.Support;

namespace SkyFocus.Tests;

[TestClass]
public sealed class StatisticsServiceTests
{
    [TestMethod]
    public async Task GetAchievementStateAsync_ComputesCurrentStreakAndBadge()
    {
        TestTextService text = new();
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        InMemorySessionRepository repository = new([
            CreateRecord(today, 25, "deep-work", FocusSessionStatus.Completed),
            CreateRecord(today.AddDays(-1), 40, "deep-work", FocusSessionStatus.Completed),
            CreateRecord(today.AddDays(-2), 30, "planning", FocusSessionStatus.Completed),
            CreateRecord(today.AddDays(-4), 20, "study-sprint", FocusSessionStatus.Completed)
        ]);
        StatisticsService service = new(repository, text);

        AchievementState achievement = await service.GetAchievementStateAsync();

        Assert.AreEqual(3, achievement.CurrentStreakDays);
        Assert.AreEqual(3, achievement.LongestStreakDays);
        Assert.AreEqual(1, achievement.CompletedToday);
        Assert.AreEqual("Cloud Cruiser", achievement.BadgeTitle);
    }

    [TestMethod]
    public async Task GetTagBreakdownAsync_FiltersToCompletedSessionsAndAggregatesMinutes()
    {
        TestTextService text = new();
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        InMemorySessionRepository repository = new([
            CreateRecord(today, 25, "deep-work", FocusSessionStatus.Completed),
            CreateRecord(today, 35, "deep-work", FocusSessionStatus.Completed),
            CreateRecord(today, 10, "planning", FocusSessionStatus.Completed),
            CreateRecord(today, 50, "planning", FocusSessionStatus.Abandoned)
        ]);
        StatisticsService service = new(repository, text);

        IReadOnlyList<TagBreakdown> breakdown = await service.GetTagBreakdownAsync(7, CatalogFactory.CreateTags(text));

        Assert.HasCount(2, breakdown);
        Assert.AreEqual("deep-work", breakdown[0].Key);
        Assert.AreEqual(60, breakdown[0].MinutesFocused);
        Assert.AreEqual("planning", breakdown[1].Key);
        Assert.AreEqual(10, breakdown[1].MinutesFocused);
    }

    private static FocusSessionRecord CreateRecord(DateOnly localDate, int minutes, string tagKey, FocusSessionStatus status)
    {
        DateTime localTime = localDate.ToDateTime(new TimeOnly(9, 0));
        DateTimeOffset localOffset = new(localTime, TimeZoneInfo.Local.GetUtcOffset(localTime));
        return new FocusSessionRecord
        {
            Id = Guid.NewGuid(),
            TagKey = tagKey,
            RouteLabel = tagKey,
            PlannedMinutes = minutes,
            ActualMinutes = minutes,
            CreatedAtUtc = localOffset.ToUniversalTime().AddMinutes(-minutes),
            StartedAtUtc = localOffset.ToUniversalTime().AddMinutes(-minutes),
            EndedAtUtc = localOffset.ToUniversalTime(),
            Status = status,
            UsedFocusShield = true,
            UsedTopMost = false,
            AmbientSoundId = "cabin-rain",
        };
    }
}
