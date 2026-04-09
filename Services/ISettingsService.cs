using System;
using System.Threading.Tasks;
using SkyFocus.Models;

namespace SkyFocus.Services;

public interface ISettingsService
{
    event EventHandler<AppSettings>? SettingsChanged;

    Task<AppSettings> GetAsync();

    Task SaveAsync(AppSettings settings);

    Task<AppSettings> UpdateAsync(Func<AppSettings, AppSettings> update);
}