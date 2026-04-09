using System.Collections.Generic;
using SkyFocus.Models;

namespace SkyFocus.Services;

internal static class CatalogFactory
{
    public static IReadOnlyList<AmbientSoundProfile> CreateSounds(ITextService textService)
    {
        return
        [
            new AmbientSoundProfile("cabin-rain", textService.GetString("SoundCabinRainName"), textService.GetString("SoundCabinRainDescription"), "ms-appx:///Assets/Audio/cabin-rain.wav"),
            new AmbientSoundProfile("jetstream-hum", textService.GetString("SoundJetstreamHumName"), textService.GetString("SoundJetstreamHumDescription"), "ms-appx:///Assets/Audio/jetstream-hum.wav"),
            new AmbientSoundProfile("night-cruise", textService.GetString("SoundNightCruiseName"), textService.GetString("SoundNightCruiseDescription"), "ms-appx:///Assets/Audio/night-cruise.wav")
        ];
    }

    public static IReadOnlyList<FocusTag> CreateTags(ITextService textService)
    {
        return
        [
            new FocusTag("deep-work", textService.GetString("TagDeepWorkName"), "\uE8D2", textService.GetString("RouteDeepWorkLabel")),
            new FocusTag("study-sprint", textService.GetString("TagStudySprintName"), "\uE7BE", textService.GetString("RouteStudySprintLabel")),
            new FocusTag("planning", textService.GetString("TagPlanningName"), "\uE823", textService.GetString("RoutePlanningLabel")),
            new FocusTag("creative-lab", textService.GetString("TagCreativeLabName"), "\uE790", textService.GetString("RouteCreativeLabLabel")),
            new FocusTag("admin-sweep", textService.GetString("TagAdminSweepName"), "\uE7F1", textService.GetString("RouteAdminSweepLabel"))
        ];
    }
}