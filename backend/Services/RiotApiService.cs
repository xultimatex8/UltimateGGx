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
            $"Summoner {username}#{tag} not found.",
            nameof(Summoner),
            $"{nameof(Summoner.Username)}/{nameof(Summoner.Tag)}",
            $"{username}#{tag}")
            ?? throw new InvalidOperationException("Could not retrieve Riot Account Info");
    }

    public async Task<AccountResponseDto> GetRiotAccountByPuuidAsync(
        string puuid,
        CancellationToken ct = default)
    {
        return await GetAsync<AccountResponseDto>(
            _regionalClient,
            $"/riot/account/v1/accounts/by-puuid/{puuid}",
            ct,
            $"Summoner not found.",
            nameof(Summoner),
            nameof(Summoner.Puuid),
            puuid)
            ?? throw new InvalidOperationException("Could not retrieve Riot Account Info");
    }

    public async Task<SummonerResponseDto> GetRiotSummonerAsync(
        string puuid,
        string? identifier = null,
        CancellationToken ct = default)
    {
        return await GetAsync<SummonerResponseDto>(
            _platformClient,
            $"/lol/summoner/v4/summoners/by-puuid/{puuid}",
            ct,
            $"Summoner{(string.IsNullOrEmpty(identifier) ? string.Empty : $" {identifier}")} not found.",
            nameof(Summoner),
            nameof(Summoner.Puuid),
            identifier ?? puuid)
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
            "We couldn't find this summoner's ranked data.",
            nameof(Summoner),
            nameof(Summoner.Puuid),
            puuid)
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
            "We couldn't find this summoner's matches.",
            nameof(Summoner),
            nameof(Summoner.Puuid),
            puuid)
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
            "Match not found.",
            nameof(Match),
            nameof(Match.MatchReference.MatchId),
            matchId)
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
            "The timeline for this match is not available.",
            nameof(Match),
            nameof(Match.MatchReference.MatchId),
            matchId)
            ?? throw new InvalidOperationException("Could not retrieve Riot Match Timeline");
    }

    private static async Task<T?> GetAsync<T>(
        HttpClient client,
        string url,
        CancellationToken ct,
        string? userMessage = null,
        string? entityName = null,
        string? propertyName = null,
        object? value = null)
    {
        try
        {
            return await client.GetFromJsonAsync<T>(url, ct);
        }
        catch (HttpRequestException ex) when (
            ex.StatusCode == HttpStatusCode.NotFound &&
            userMessage is not null)
        {
            throw new NotFoundException(
                userMessage,
                entityName!,
                propertyName!,
                value!);
        }
        catch (HttpRequestException ex)
        {
            HttpStatusCode statusCode = ex.StatusCode ?? HttpStatusCode.InternalServerError;
            throw new RiotApiException(statusCode, url, ex);
        }
    }
}