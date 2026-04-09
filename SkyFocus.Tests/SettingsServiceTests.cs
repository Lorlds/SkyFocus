using SkyFocus.Models;
using SkyFocus.Services;
using SkyFocus.Tests.Support;

namespace SkyFocus.Tests;

[TestClass]
public sealed class SettingsServiceTests
{
    [TestMethod]
    public async Task SaveAsync_PersistsSettingsToDisk()
    {
        using TempStoragePathProvider storage = new();
        SettingsService first = new(storage);
        AppSettings expected = new()
        {
            DefaultSessionMinutes = 45,
            DefaultTagKey = "planning",
            Theme = "Dark",
            EnableSoftBlocker = true,
            UseFocusShield = false,
            KeepWindowOnTop = true,
            SendSystemNotifications = false,
            NotifyAtFiveMinutes = false,
            AmbientVolume = 0.72,
            SelectedAmbientSoundId = "night-cruise",
        };

        await first.SaveAsync(expected);

        SettingsService second = new(storage);
        AppSettings actual = await second.GetAsync();

        Assert.AreEqual(expected.DefaultSessionMinutes, actual.DefaultSessionMinutes);
        Assert.AreEqual(expected.DefaultTagKey, actual.DefaultTagKey);
        Assert.AreEqual(expected.Theme, actual.Theme);
        Assert.AreEqual(expected.KeepWindowOnTop, actual.KeepWindowOnTop);
        Assert.AreEqual(expected.SendSystemNotifications, actual.SendSystemNotifications);
        Assert.AreEqual(expected.NotifyAtFiveMinutes, actual.NotifyAtFiveMinutes);
        Assert.AreEqual(expected.AmbientVolume, actual.AmbientVolume, 0.001);
        Assert.AreEqual(expected.SelectedAmbientSoundId, actual.SelectedAmbientSoundId);
    }

    [TestMethod]
    public async Task UpdateAsync_RaisesSettingsChangedEvent()
    {
        using TempStoragePathProvider storage = new();
        SettingsService service = new(storage);
        AppSettings? observed = null;
        service.SettingsChanged += (_, settings) => observed = settings;

        AppSettings updated = await service.UpdateAsync(current => current with
        {
            DefaultSessionMinutes = 60,
            Theme = "Light",
            KeepWindowOnTop = true,
        });

        Assert.IsNotNull(observed);
        Assert.AreEqual(60, updated.DefaultSessionMinutes);
        Assert.AreEqual("Light", updated.Theme);
        Assert.IsTrue(updated.KeepWindowOnTop);
        Assert.AreEqual(updated, observed);
    }
}