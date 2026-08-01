using backend.Models;
using backend.Models.Dtos;

namespace backend.Interfaces;

public interface ITimelineService
{
    Task CheckOrFetchTimelineAsync(string matchId, CancellationToken ct = default);
    Task SyncTimelineAsync(string matchId, CancellationToken ct = default);
    Task<TimelineDto> GetTimelineAsync(string matchId, CancellationToken ct = default);
    Task<ScoreboardDto> GetScoreboardAsync(string matchId, long timestamp, CancellationToken ct = default);
}