using backend.Models.DataDragon;

namespace backend.Interfaces;

public interface IDataDragonService
{
    Task<string> GetLatestVersionAsync(CancellationToken ct = default);
    Task<ChampionResponseDto> GetChampionsAsync(string version, CancellationToken ct = default);
    Task<SummonerSpellResponseDto> GetSummonerSpellsAsync(string version, CancellationToken ct = default);
    Task<ItemResponseDto> GetItemsAsync(string version, CancellationToken ct = default);
}