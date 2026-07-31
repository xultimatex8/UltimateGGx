using backend.Models;

namespace backend.Interfaces;

public interface ITimelineService
{
    Task GetOrFetchTimelineAsync(string matchId, CancellationToken ct = default);
    Task SyncTimelineAsync(string matchId, CancellationToken ct = default);
}