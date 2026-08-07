using System.Net;
using System.Net.Http;
using backend.Services;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using backend.Models.Enums;
using backend.Exceptions;

namespace backend.Tests.Services;

public class RiotApiServiceTests
{
    private static (RiotApiService service, MockHttpMessageHandler mockHttp) CreateService()
    {
        var mockHttp = new MockHttpMessageHandler();

        var services = new ServiceCollection();
        services.AddHttpClient("RiotPlatform", c => c.BaseAddress = new Uri("https://euw1.api.riotgames.com"))
            .ConfigurePrimaryHttpMessageHandler(() => mockHttp);
        services.AddHttpClient("RiotRegional", c => c.BaseAddress = new Uri("https://europe.api.riotgames.com"))
            .ConfigurePrimaryHttpMessageHandler(() => mockHttp);

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();

        return (new RiotApiService(factory), mockHttp);
    }

    [Fact]
    public async Task GetRiotAccountAsync_ReturnsAccountInfo()
    {
        var (service, mockHttp) = CreateService();

        mockHttp.When("https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/Faker/KR1")
            .Respond("application/json", """
                { "puuid": "abc-123", "gameName": "Faker", "tagLine": "KR1" }
                """);

        var account = await service.GetRiotAccountAsync("Faker", "KR1");

        account.Puuid.Should().Be("abc-123");
        account.GameName.Should().Be("Faker");
        account.TagLine.Should().Be("KR1");
    }

    [Fact]
    public async Task GetRiotAccountAsync_WhenNotFound_ThrowsNotFoundException()
    {
        var (service, mockHttp) = CreateService();

        mockHttp.When("https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/Nobody/000")
            .Respond(HttpStatusCode.NotFound);

        Func<Task> act = () => service.GetRiotAccountAsync("Nobody", "000");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetRiotAccountAsync_WhenRateLimited_ThrowsRiotApiExceptionWithTooManyRequests()
    {
        var (service, mockHttp) = CreateService();

        mockHttp.When("https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/Faker/KR1")
            .Respond((HttpStatusCode)429);

        Func<Task> act = async () => await service.GetRiotAccountAsync("Faker", "KR1");

        var exception = await act.Should().ThrowAsync<RiotApiException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task GetRiotSummonerAsync_ReturnsSummonerInfo()
    {
        var (service, mockHttp) = CreateService();

        mockHttp.When("https://euw1.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/abc-123")
            .Respond("application/json", """
                { "id": "sum-1", "profileIconId": 42, "summonerLevel": 300 }
                """);

        var summoner = await service.GetRiotSummonerAsync("abc-123");

        summoner.ProfileIconId.Should().Be(42);
        summoner.SummonerLevel.Should().Be(300);
    }

    [Fact]
    public async Task GetSummonerQueuesAsync_ReturnsListOfQueues()
    {
        var (service, mockHttp) = CreateService();

        mockHttp.When("https://euw1.api.riotgames.com/lol/league/v4/entries/by-puuid/abc-123")
            .Respond("application/json", """
                [
                  {
                    "queueType": "RANKED_SOLO_5x5",
                    "tier": "GOLD",
                    "rank": "II",
                    "leaguePoints": 55,
                    "wins": 120,
                    "losses": 100
                  }
                ]
                """);

        var queues = await service.GetSummonerQueuesAsync("abc-123");

        queues.Should().HaveCount(1);
        queues[0].QueueType.Should().Be("RANKED_SOLO_5x5");
        queues[0].Wins.Should().Be(120);
    }

    [Fact]
    public async Task GetSummonerQueuesAsync_WhenEmpty_ReturnsEmptyList()
    {
        var (service, mockHttp) = CreateService();

        mockHttp.When("https://euw1.api.riotgames.com/lol/league/v4/entries/by-puuid/no-ranked")
            .Respond("application/json", "[]");

        var queues = await service.GetSummonerQueuesAsync("no-ranked");

        queues.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSummonerMatchesAsync_ReturnsListOfMatchIds()
    {
        var (service, mockHttp) = CreateService();

        mockHttp.When("https://europe.api.riotgames.com/lol/match/v5/matches/by-puuid/abc-123/ids?queue=400&count=10")
            .Respond("application/json", """
                ["EUW1_1111111111", "EUW1_2222222222"]
                """);

        var matchIds = await service.GetSummonerMatchesAsync("abc-123", QueueType.DRAFT_PICK);

        matchIds.Should().HaveCount(2);
        matchIds.Should().Contain("EUW1_1111111111");
        matchIds.Should().Contain("EUW1_2222222222");
    }

    [Fact]
    public async Task GetSummonerMatchesAsync_WhenEmpty_ReturnsEmptyList()
    {
        var (service, mockHttp) = CreateService();

        mockHttp.When("https://europe.api.riotgames.com/lol/match/v5/matches/by-puuid/no-matches/ids?queue=400&count=10")
            .Respond("application/json", "[]");

        var matchIds = await service.GetSummonerMatchesAsync("no-matches", QueueType.DRAFT_PICK);

        matchIds.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSummonerMatchesAsync_WhenServerErrors_ThrowsRiotApiException()
    {
        var (service, mockHttp) = CreateService();

        mockHttp.When("https://europe.api.riotgames.com/lol/match/v5/matches/by-puuid/abc-123/ids?queue=400&count=10")
            .Respond(HttpStatusCode.InternalServerError);

        Func<Task> act = async () => await service.GetSummonerMatchesAsync("abc-123", QueueType.DRAFT_PICK);

        var exception = await act.Should().ThrowAsync<RiotApiException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetMatchDetailAsync_ReturnsMatchResponse()
    {
        var (service, mockHttp) = CreateService();

        mockHttp.When("https://europe.api.riotgames.com/lol/match/v5/matches/EUW1_1111111111")
            .Respond("application/json", """
                {
                  "info": {
                    "endOfGameResult": "GameComplete",
                    "gameDuration": 1800,
                    "gameEndTimestamp": 1700000000,
                    "queueId": 400,
                    "participants": [],
                    "teams": [
                      { "teamId": 100, "win": true },
                      { "teamId": 200, "win": false }
                    ]
                  }
                }
                """);

        var result = await service.GetMatchDetailAsync("EUW1_1111111111");

        result.Info.EndOfGameResult.Should().Be("GameComplete");
        result.Info.GameDuration.Should().Be(1800);
        result.Info.Teams.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMatchDetailAsync_WhenNotFound_ThrowsRiotApiException()
    {
        var (service, mockHttp) = CreateService();

        mockHttp.When("https://europe.api.riotgames.com/lol/match/v5/matches/NOT_A_REAL_MATCH")
            .Respond(HttpStatusCode.NotFound);

        Func<Task> act = async () => await service.GetMatchDetailAsync("NOT_A_REAL_MATCH");

        await act.Should().ThrowAsync<NotFoundException>();
    }
}