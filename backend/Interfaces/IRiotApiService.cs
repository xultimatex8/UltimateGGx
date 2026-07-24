using backend.Models.Riot;

namespace backend.Interfaces;

public interface IRiotApiService
{
    Task<AccountResponseDto> GetRiotAccountAsync(string username, string tag, CancellationToken ct = default);
    Task<SummonerResponseDto> GetRiotSummonerAsync(string puuid, CancellationToken ct = default);
    Task<List<QueueResponseDto>> GetSummonerQueuesAsync(string puuid, CancellationToken ct = default);
}