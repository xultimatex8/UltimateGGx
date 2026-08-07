using backend.Data;
using backend.Interfaces;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class DataDragonSyncCheckerService : IDataDragonSyncCheckerService
{
    private readonly IDataDragonService _dataDragonService;
    private readonly ChampionSyncService _syncService;
    private readonly AppDbContext _db;
    private readonly ILogger<DataDragonSyncCheckerService> _logger;

    public DataDragonSyncCheckerService(
        IDataDragonService dataDragonService,
        ChampionSyncService syncService,
        AppDbContext db,
        ILogger<DataDragonSyncCheckerService> logger)
    {
        _dataDragonService = dataDragonService;
        _syncService = syncService;
        _db = db;
        _logger = logger;
    }

    public async Task CheckAndSyncAsync(CancellationToken ct = default)
    {
        try
        {
            string latestVersion = await _dataDragonService.GetLatestVersionAsync(ct);

            DataDragonState? state = await _db.DataDragonState.FirstOrDefaultAsync(ct);

            if (state is null)
            {
                state = new DataDragonState { CurrentVersion = "" };
                _db.DataDragonState.Add(state);
            }

            if (state.CurrentVersion != latestVersion)
            {
                _logger.LogInformation(
                    "New Data Dragon version detected: {Old} -> {New}",
                    state.CurrentVersion, latestVersion);

                await _syncService.SyncAsync(ct);

                state.CurrentVersion = latestVersion;
            }

            state.LastCheckedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Data Dragon version");
        }
    }
}