using backend.Interfaces;

namespace backend.Services;

public class DataDragonSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6);

    public DataDragonSyncBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(_checkInterval);

        do
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            IDataDragonSyncCheckerService checker = scope.ServiceProvider.GetRequiredService<IDataDragonSyncCheckerService>();
            await checker.CheckAndSyncAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}