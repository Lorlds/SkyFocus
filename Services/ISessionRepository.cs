using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkyFocus.Models;

namespace SkyFocus.Services;

internal interface ISessionRepository
{
    Task InitializeAsync();

    Task AddSessionAsync(FocusSessionRecord record);

    Task<IReadOnlyList<FocusSessionRecord>> GetRecentSessionsAsync(int count);

    Task<IReadOnlyList<FocusSessionRecord>> GetSessionsAsync(DateTimeOffset? fromUtc = null);
}