using backend.Models.Dtos;
using backend.Models.Enums;

namespace backend.Interfaces;

public interface IMatchService
{
    Task FetchSummonerMatchesAsync(string puuid, QueueType queueType = QueueType.DRAFT_PICK, CancellationToken ct = default);
    Task<List<MatchDto>> GetSummonerMatchesAsync(string puuid, QueueType queueType = QueueType.DRAFT_PICK, CancellationToken ct = default);
}