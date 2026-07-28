using backend.Interfaces;
using backend.Models.Enums;
using backend.Helpers;
using backend.Models.Riot;

namespace backend.Services;

public class RiotApiService : IRiotApiService
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

    public async Task<List<string>> GetSummonerMatchesAsync(string puuid, QueueType type, CancellationToken ct = default)
    {
        List<string>? response = await _regionalClient.GetFromJsonAsync<List<string>>(
            $"/lol/match/v5/matches/by-puuid/{puuid}/ids?queue={QueueTypeHelper.QueueTypeToQueueId(type)}&count=10", ct);

        return response ?? throw new Exception("Could not retrieve Riot Summoner Matches");
    }

    public async Task<MatchResponseDto> GetMatchDetailAsync(string matchId, CancellationToken ct = default)
    {
        MatchResponseDto? response = await _regionalClient.GetFromJsonAsync<MatchResponseDto>(
            $"/lol/match/v5/matches/{matchId}", ct);

        return response ?? throw new Exception("Could not retrieve Riot Match Detail");
    }
}