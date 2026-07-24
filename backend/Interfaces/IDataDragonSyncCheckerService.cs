namespace backend.Interfaces;

public interface IDataDragonSyncCheckerService
{
    Task CheckAndSyncAsync(CancellationToken ct = default);
}