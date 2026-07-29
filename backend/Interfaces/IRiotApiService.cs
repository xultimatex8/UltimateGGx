using backend.Models.Enums;
using backend.Models.Riot;

namespace backend.Interfaces;

public interface IRiotApiService
{
    Task<AccountResponseDto> GetRiotAccountAsync(string username, string tag, CancellationToken ct = default);
    Task<SummonerResponseDto> GetRiotSummonerAsync(string puuid, CancellationToken ct = default);
    Task<List<QueueResponseDto>> GetSummonerQueuesAsync(string puuid, CancellationToken ct = default);
    Task<List<string>> GetSummonerMatchesAsync(string puuid, QueueType type, CancellationToken ct = default);
    Task<MatchResponseDto> GetMatchDetailAsync(string matchId, CancellationToken ct = default);
    Task<TimelineResponseDto> GetMatchTimelineAsync(string matchId, CancellationToken ct = default);
}