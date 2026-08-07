using backend.Models.Dtos;

namespace backend.Interfaces;

public interface ISummonerService
{
    Task<SummonerDto> GetOrFetchSummonerAsync(string username, string tag, CancellationToken ct = default);
    Task<SummonerDto> SyncSummonerAsync(string username, string tag, CancellationToken ct = default);
    Task<SummonerDto> SyncSummonerByPuuidAsync(string puuid, CancellationToken ct = default);
}