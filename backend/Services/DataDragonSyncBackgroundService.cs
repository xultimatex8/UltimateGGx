using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.BackgroundServices;

public class DataDragonSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataDragonSyncBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6);

    public DataDragonSyncBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DataDragonSyncBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_checkInterval);

        do
        {
            await CheckAndSyncAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CheckAndSyncAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var dataDragonService = scope.ServiceProvider.GetRequiredService<DataDragonService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            var latestVersion = await dataDragonService.GetLatestVersionAsync(ct);

            var state = await db.DataDragonState.FirstOrDefaultAsync(ct);

            if (state is null)
            {
                state = new DataDragonState { CurrentVersion = "" };
                db.DataDragonState.Add(state);
            }

            if (state.CurrentVersion != latestVersion)
            {
                _logger.LogInformation(
                    "New Data Dragon version detected: {Old} -> {New}",
                    state.CurrentVersion, latestVersion);

                state.CurrentVersion = latestVersion;
            }

            state.LastCheckedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Data Dragon version");
        }
    }
}