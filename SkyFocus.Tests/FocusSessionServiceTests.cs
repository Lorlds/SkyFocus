using SkyFocus.Models;
using SkyFocus.Services;
using SkyFocus.Tests.Support;

namespace SkyFocus.Tests;

[TestClass]
public sealed class FocusSessionServiceTests
{
    [TestMethod]
    public async Task CreateStartCompleteAsync_PersistsCompletedSessionAndUpdatesState()
    {
        InMemorySessionRepository repository = new();
        TestSettingsService settings = new(new AppSettings { SendSystemNotifications = false, EnableSoftBlocker = true, UseFocusShield = true, KeepWindowOnTop = true });
        TestReminderService reminders = new();
        TestBlockerService blocker = new();
        TestTextService text = new();
        FocusSessionService service = new(repository, settings, reminders, blocker, text);

        FocusSessionSnapshot created = await service.CreateSessionAsync(25, "deep-work", "cabin-rain");
        await service.StartSessionAsync();
        await service.CompleteSessionAsync();

        Assert.AreEqual(FocusSessionStatus.Completed, service.Snapshot.Status);
        Assert.AreEqual(created.SessionId, service.Snapshot.SessionId);
        Assert.HasCount(1, repository.Records);
        Assert.AreEqual(FocusSessionStatus.Completed, repository.Records[0].Status);
        Assert.AreEqual("deep-work", repository.Records[0].TagKey);
        Assert.AreEqual("Atlantic Focus Corridor", repository.Records[0].RouteLabel);
        Assert.AreEqual(1, reminders.StartedCount);
        Assert.AreEqual(1, reminders.CompletedCount);
        Assert.IsGreaterThanOrEqualTo(blocker.UpdateCallCount, 3);
        Assert.IsFalse(blocker.State.ShouldUseFullScreen);
    }

    [TestMethod]
    public async Task PauseResumeAbandonAsync_TransitionsThroughExpectedStatuses()
    {
        InMemorySessionRepository repository = new();
        TestSettingsService settings = new(new AppSettings { SendSystemNotifications = false });
        TestReminderService reminders = new();
        TestBlockerService blocker = new();
        TestTextService text = new();
        FocusSessionService service = new(repository, settings, reminders, blocker, text);

        await service.CreateSessionAsync(45, "planning", "jetstream-hum");
        await service.StartSessionAsync();
        await service.PauseSessionAsync();
        Assert.AreEqual(FocusSessionStatus.Paused, service.Snapshot.Status);

        await service.ResumeSessionAsync();
        Assert.AreEqual(FocusSessionStatus.Running, service.Snapshot.Status);

        await service.AbandonSessionAsync();

        Assert.AreEqual(FocusSessionStatus.Abandoned, service.Snapshot.Status);
        Assert.HasCount(1, repository.Records);
        Assert.AreEqual(FocusSessionStatus.Abandoned, repository.Records[0].Status);
        Assert.AreEqual(1, reminders.StartedCount);
        Assert.AreEqual(0, reminders.CompletedCount);
    }
}
