using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SkyFocus.Models;

namespace SkyFocus.Services;

internal sealed class SessionRepository : ISessionRepository
{
    private readonly string _databasePath;
    private bool _initialized;

    public SessionRepository(IStoragePathProvider storagePathProvider)
    {
        _databasePath = Path.Combine(storagePathProvider.GetLocalDataPath(), "skyfocus.db");
    }

    public async Task AddSessionAsync(FocusSessionRecord record)
    {
        await InitializeAsync().ConfigureAwait(false);

        await using SqliteConnection connection = CreateConnection();
        await connection.OpenAsync().ConfigureAwait(false);

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Sessions (Id, TagKey, RouteLabel, PlannedMinutes, ActualMinutes, CreatedAtUtc, StartedAtUtc, EndedAtUtc, Status, UsedFocusShield, UsedTopMost, AmbientSoundId) VALUES ($id, $tagKey, $routeLabel, $plannedMinutes, $actualMinutes, $createdAtUtc, $startedAtUtc, $endedAtUtc, $status, $usedFocusShield, $usedTopMost, $ambientSoundId);";
        command.Parameters.AddWithValue("$id", record.Id.ToString());
        command.Parameters.AddWithValue("$tagKey", record.TagKey);
        command.Parameters.AddWithValue("$routeLabel", record.RouteLabel);
        command.Parameters.AddWithValue("$plannedMinutes", record.PlannedMinutes);
        command.Parameters.AddWithValue("$actualMinutes", record.ActualMinutes);
        command.Parameters.AddWithValue("$createdAtUtc", record.CreatedAtUtc.ToString("o", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$startedAtUtc", record.StartedAtUtc?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty);
        command.Parameters.AddWithValue("$endedAtUtc", record.EndedAtUtc.ToString("o", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$status", (int)record.Status);
        command.Parameters.AddWithValue("$usedFocusShield", record.UsedFocusShield ? 1 : 0);
        command.Parameters.AddWithValue("$usedTopMost", record.UsedTopMost ? 1 : 0);
        command.Parameters.AddWithValue("$ambientSoundId", record.AmbientSoundId);
        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_databasePath)!);

        await using SqliteConnection connection = CreateConnection();
        await connection.OpenAsync().ConfigureAwait(false);

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS Sessions (Id TEXT PRIMARY KEY, TagKey TEXT NOT NULL, RouteLabel TEXT NOT NULL, PlannedMinutes INTEGER NOT NULL, ActualMinutes INTEGER NOT NULL, CreatedAtUtc TEXT NOT NULL, StartedAtUtc TEXT NULL, EndedAtUtc TEXT NOT NULL, Status INTEGER NOT NULL, UsedFocusShield INTEGER NOT NULL, UsedTopMost INTEGER NOT NULL, AmbientSoundId TEXT NOT NULL);";
        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        _initialized = true;
    }

    public async Task<IReadOnlyList<FocusSessionRecord>> GetRecentSessionsAsync(int count)
    {
        await InitializeAsync().ConfigureAwait(false);
        List<FocusSessionRecord> records = [];

        await using SqliteConnection connection = CreateConnection();
        await connection.OpenAsync().ConfigureAwait(false);

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Id, TagKey, RouteLabel, PlannedMinutes, ActualMinutes, CreatedAtUtc, StartedAtUtc, EndedAtUtc, Status, UsedFocusShield, UsedTopMost, AmbientSoundId FROM Sessions ORDER BY EndedAtUtc DESC LIMIT $count;";
        command.Parameters.AddWithValue("$count", count);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            records.Add(ReadRecord(reader));
        }

        return records;
    }

    public async Task<IReadOnlyList<FocusSessionRecord>> GetSessionsAsync(DateTimeOffset? fromUtc = null)
    {
        await InitializeAsync().ConfigureAwait(false);
        List<FocusSessionRecord> records = [];

        await using SqliteConnection connection = CreateConnection();
        await connection.OpenAsync().ConfigureAwait(false);

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Id, TagKey, RouteLabel, PlannedMinutes, ActualMinutes, CreatedAtUtc, StartedAtUtc, EndedAtUtc, Status, UsedFocusShield, UsedTopMost, AmbientSoundId FROM Sessions WHERE ($fromUtc = '' OR EndedAtUtc >= $fromUtc) ORDER BY EndedAtUtc ASC;";
        command.Parameters.AddWithValue("$fromUtc", fromUtc?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            records.Add(ReadRecord(reader));
        }

        return records;
    }

    private static FocusSessionRecord ReadRecord(SqliteDataReader reader)
    {
        string? startedAtUtc = reader.GetString(6);

        return new FocusSessionRecord
        {
            Id = Guid.Parse(reader.GetString(0)),
            TagKey = reader.GetString(1),
            RouteLabel = reader.GetString(2),
            PlannedMinutes = reader.GetInt32(3),
            ActualMinutes = reader.GetInt32(4),
            CreatedAtUtc = DateTimeOffset.Parse(reader.GetString(5), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            StartedAtUtc = string.IsNullOrWhiteSpace(startedAtUtc) ? null : DateTimeOffset.Parse(startedAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            EndedAtUtc = DateTimeOffset.Parse(reader.GetString(7), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            Status = (FocusSessionStatus)reader.GetInt32(8),
            UsedFocusShield = reader.GetInt32(9) == 1,
            UsedTopMost = reader.GetInt32(10) == 1,
            AmbientSoundId = reader.GetString(11),
        };
    }

    private SqliteConnection CreateConnection()
    {
        return new SqliteConnection($"Data Source={_databasePath}");
    }
}