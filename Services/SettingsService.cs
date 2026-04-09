using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SkyFocus.Models;

namespace SkyFocus.Services;

internal sealed class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    private readonly string _settingsPath;
    private AppSettings? _cachedSettings;

    public SettingsService(IStoragePathProvider storagePathProvider)
    {
        _settingsPath = Path.Combine(storagePathProvider.GetLocalDataPath(), "settings.json");
    }

    public event EventHandler<AppSettings>? SettingsChanged;

    public async Task<AppSettings> GetAsync()
    {
        if (_cachedSettings is not null)
        {
            return _cachedSettings;
        }

        if (!File.Exists(_settingsPath))
        {
            _cachedSettings = new AppSettings();
            await SaveAsync(_cachedSettings).ConfigureAwait(false);
            return _cachedSettings;
        }

        await using FileStream stream = File.OpenRead(_settingsPath);
        _cachedSettings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, SerializerOptions).ConfigureAwait(false) ?? new AppSettings();
        return _cachedSettings;
    }

    public async Task SaveAsync(AppSettings settings)
    {
        _cachedSettings = settings;
        await using FileStream stream = File.Create(_settingsPath);
        await JsonSerializer.SerializeAsync(stream, settings, SerializerOptions).ConfigureAwait(false);
        await stream.FlushAsync().ConfigureAwait(false);
        SettingsChanged?.Invoke(this, settings);
    }

    public async Task<AppSettings> UpdateAsync(Func<AppSettings, AppSettings> update)
    {
        AppSettings current = await GetAsync().ConfigureAwait(false);
        AppSettings next = update(current);
        await SaveAsync(next).ConfigureAwait(false);
        return next;
    }
}