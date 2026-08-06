using backend.Exceptions;
using backend.Interfaces;
using backend.Models.Enums;
using backend.Helpers;
using backend.Models.Riot;
using System.Net;
using backend.Models;

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

    public async Task<AccountResponseDto> GetRiotAccountAsync(
        string username,
        string tag,
        CancellationToken ct = default)
    {
        return await GetAsync<AccountResponseDto>(
            _regionalClient,
            $"/riot/account/v1/accounts/by-riot-id/{username}/{tag}",
            ct,
            $"{nameof(Summoner)} \"{username}#{tag}\" not found.")
            ?? throw new InvalidOperationException("Could not retrieve Riot Account Info");
    }

    public async Task<SummonerResponseDto> GetRiotSummonerAsync(
        string puuid,
        CancellationToken ct = default)
    {
        return await GetAsync<SummonerResponseDto>(
            _platformClient,
            $"/lol/summoner/v4/summoners/by-puuid/{puuid}",
            ct,
            $"{nameof(Summoner)} \"{puuid}\" not found.")
            ?? throw new InvalidOperationException("Could not retrieve Riot Summoner Info");
    }

    public async Task<List<QueueResponseDto>> GetSummonerQueuesAsync(
        string puuid,
        CancellationToken ct = default)
    {
        return await GetAsync<List<QueueResponseDto>>(
            _platformClient,
            $"/lol/league/v4/entries/by-puuid/{puuid}",
            ct,
            $"{nameof(Summoner)} \"{puuid}\" not found.")
            ?? throw new InvalidOperationException("Could not retrieve Riot Summoner Queues Info");
    }

    public async Task<List<string>> GetSummonerMatchesAsync(
        string puuid,
        QueueType type,
        CancellationToken ct = default)
    {
        return await GetAsync<List<string>>(
            _regionalClient,
            $"/lol/match/v5/matches/by-puuid/{puuid}/ids?queue={QueueTypeHelper.QueueTypeToQueueId(type)}&count=10",
            ct,
            $"{nameof(Summoner)} \"{puuid}\" not found.")
            ?? throw new InvalidOperationException("Could not retrieve Riot Summoner Matches");
    }

    public async Task<MatchResponseDto> GetMatchDetailAsync(
        string matchId,
        CancellationToken ct = default)
    {
        return await GetAsync<MatchResponseDto>(
            _regionalClient,
            $"/lol/match/v5/matches/{matchId}",
            ct,
            $"{nameof(Match)} \"{matchId}\" not found.")
            ?? throw new InvalidOperationException("Could not retrieve Riot Match Detail");
    }

    public async Task<TimelineResponseDto> GetMatchTimelineAsync(
        string matchId,
        CancellationToken ct = default)
    {
        return await GetAsync<TimelineResponseDto>(
            _regionalClient,
            $"/lol/match/v5/matches/{matchId}/timeline",
            ct,
            $"{nameof(Match)} \"{matchId}\" not found.")
            ?? throw new InvalidOperationException("Could not retrieve Riot Match Timeline");
    }

    private static async Task<T?> GetAsync<T>(
        HttpClient client,
        string url,
        CancellationToken ct,
        string? notFoundMessage = null)
    {
        try
        {
            return await client.GetFromJsonAsync<T>(url, ct);
        }
        catch (HttpRequestException ex) when (
            ex.StatusCode == HttpStatusCode.NotFound &&
            notFoundMessage is not null)
        {
            throw new NotFoundException(notFoundMessage);
        }
        catch (HttpRequestException ex)
        {
            HttpStatusCode statusCode = ex.StatusCode ?? HttpStatusCode.InternalServerError;
            throw new RiotApiException(statusCode, url, ex);
        }
    }
}