using backend.Models;
using backend.Models.Dtos;
using backend.Models.Enums;

namespace backend.Interfaces;

public interface IMatchService
{
    Task FetchSummonerMatchesAsync(string puuid, QueueType queueType = QueueType.DRAFT_PICK, CancellationToken ct = default);
    Task<PagedResult<MatchDto>> GetSummonerMatchesAsync(string puuid, int page = 1, int pageSize = 10, QueueType queueType = QueueType.DRAFT_PICK, CancellationToken ct = default);
    Task<Match> GetOrCreateMatchAsync(string matchId, CancellationToken ct = default);
}