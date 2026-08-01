using backend.Interfaces;
using backend.Models.DataDragon;

namespace backend.Services;

public class DataDragonService : IDataDragonService
{
    private readonly HttpClient _httpClient;

    public DataDragonService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetLatestVersionAsync(CancellationToken ct = default)
    {
        List<string>? versions = await _httpClient.GetFromJsonAsync<List<string>>(
            "api/versions.json", ct);
        return versions?.FirstOrDefault() ?? throw new Exception("Could not retrieve Data Dragon version");
    }

    public async Task<ChampionResponseDto> GetChampionsAsync(string version, CancellationToken ct = default)
    {
        ChampionResponseDto? response = await _httpClient.GetFromJsonAsync<ChampionResponseDto>(
            $"cdn/{version}/data/en_US/champion.json", ct);
        return response ?? throw new Exception("Could not retrieve champions data");
    }

    public async Task<SummonerSpellResponseDto> GetSummonerSpellsAsync(string version, CancellationToken ct = default)
    {
        SummonerSpellResponseDto? response = await _httpClient.GetFromJsonAsync<SummonerSpellResponseDto>(
            $"cdn/{version}/data/en_US/summoner.json", ct);
        return response ?? throw new Exception("Could not retrieve summoner spells data");
    }

    public async Task<ItemResponseDto> GetItemsAsync(string version, CancellationToken ct = default)
    {
        ItemResponseDto? response = await _httpClient.GetFromJsonAsync<ItemResponseDto>(
            $"cdn/{version}/data/en_US/item.json", ct);
        return response ?? throw new Exception("Could not retrieve items data");
    }
}