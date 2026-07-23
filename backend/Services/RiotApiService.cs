using backend.Models.Riot;

namespace backend.Services;

public class RiotApiService
{
    private readonly HttpClient _platformClient;
    private readonly HttpClient _regionalClient;

    public RiotApiService(IHttpClientFactory httpClientFactory)
    {
        _platformClient = httpClientFactory.CreateClient("RiotPlatform");
        _regionalClient = httpClientFactory.CreateClient("RiotRegional");
    }

    public async Task<AccountResponseDto> GetRiotAccountAsync(string username, string tag, CancellationToken ct = default)
    {
        AccountResponseDto? response = await _regionalClient.GetFromJsonAsync<AccountResponseDto>(
            $"/riot/account/v1/accounts/by-riot-id/{username}/{tag}", ct);
        return response ?? throw new Exception("Could not retrieve Riot Account Info");
    }

    public async Task<SummonerResponseDto> GetRiotSummonerAsync(string puuid, CancellationToken ct = default)
    {
        SummonerResponseDto? response = await _platformClient.GetFromJsonAsync<SummonerResponseDto>(
            $"/lol/summoner/v4/summoners/by-puuid/{puuid}", ct);
        return response ?? throw new Exception("Could not retrieve Riot Summoner Info");
    }

    public async Task<List<QueueResponseDto>> GetSummonerQueuesAsync(string puuid, CancellationToken ct = default)
    {
        List<QueueResponseDto>? response = await _platformClient.GetFromJsonAsync<List<QueueResponseDto>>(
            $"/lol/league/v4/entries/by-puuid/{puuid}", ct);
        return response ?? throw new Exception("Could not retrieve Riot Summoner Queues Info");
    }
}