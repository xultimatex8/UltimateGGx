using System.Net;
using backend.Services;
using AwesomeAssertions;
using RichardSzalay.MockHttp;
using Xunit;

namespace backend.Tests.Services;

public class DataDragonServiceTests
{
    private static DataDragonService CreateService(MockHttpMessageHandler mockHttp)
    {
        var httpClient = new HttpClient(mockHttp)
        {
            BaseAddress = new Uri("https://ddragon.leagueoflegends.com/")
        };
        return new DataDragonService(httpClient);
    }

    [Fact]
    public async Task GetLatestVersionAsync_ReturnsFirstVersionInList()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("https://ddragon.leagueoflegends.com/api/versions.json")
            .Respond("application/json", "[\"14.14.1\", \"14.13.1\", \"14.12.1\"]");

        var service = CreateService(mockHttp);

        string version = await service.GetLatestVersionAsync();

        version.Should().Be("14.14.1");
    }

    [Fact]
    public async Task GetLatestVersionAsync_WhenResponseIsNull_Throws()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("https://ddragon.leagueoflegends.com/api/versions.json")
            .Respond("application/json", "null");

        var service = CreateService(mockHttp);

        Func<Task> act = async () => await service.GetLatestVersionAsync();

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Could not retrieve Data Dragon version");
    }

    [Fact]
    public async Task GetLatestVersionAsync_WhenServerErrors_ThrowsHttpRequestException()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("https://ddragon.leagueoflegends.com/api/versions.json")
            .Respond(HttpStatusCode.InternalServerError);

        var service = CreateService(mockHttp);

        Func<Task> act = async () => await service.GetLatestVersionAsync();

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetChampionsAsync_ParsesChampionData()
    {
        const string version = "14.14.1";
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When($"https://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/champion.json")
            .Respond("application/json", """
                {
                  "type": "champion",
                  "format": "standAloneComplex",
                  "version": "14.14.1",
                  "data": {
                    "Ahri": {
                      "id": "Ahri",
                      "key": "103",
                      "name": "Ahri",
                      "tags": ["Mage", "Assassin"]
                    }
                  }
                }
                """);

        var service = CreateService(mockHttp);

        var result = await service.GetChampionsAsync(version);

        result.Data.Should().ContainKey("Ahri");
        result.Data["Ahri"].Name.Should().Be("Ahri");
        result.Data["Ahri"].Key.Should().Be("103");
        result.Data["Ahri"].Tags.Should().BeEquivalentTo(["Mage", "Assassin"]);
    }

    [Fact]
    public async Task GetChampionsAsync_WhenResponseIsNull_Throws()
    {
        const string version = "14.14.1";
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When($"https://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/champion.json")
            .Respond("application/json", "null");

        var service = CreateService(mockHttp);

        Func<Task> act = async () => await service.GetChampionsAsync(version);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Could not retrieve champions data");
    }

    [Fact]
    public async Task GetSummonerSpellsAsync_ParsesSpellData()
    {
        const string version = "14.14.1";
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When($"https://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/summoner.json")
            .Respond("application/json", """
                {
                  "type": "summoner",
                  "version": "14.14.1",
                  "data": {
                    "SummonerFlash": {
                      "id": "SummonerFlash",
                      "key": "4",
                      "name": "Flash"
                    }
                  }
                }
                """);

        var service = CreateService(mockHttp);

        var result = await service.GetSummonerSpellsAsync(version);

        result.Data.Should().ContainKey("SummonerFlash");
        result.Data["SummonerFlash"].Name.Should().Be("Flash");
    }

    [Fact]
    public async Task GetItemsAsync_ParsesItemData()
    {
        const string version = "14.14.1";
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When($"https://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/item.json")
            .Respond("application/json", """
                {
                "type": "item",
                "version": "14.14.1",
                "data": {
                    "1001": {
                    "name": "Boots",
                    "description": "Slightly increases Movement Speed.",
                    "gold": { "total": 300, "sell": 210 },
                    "stats": { "FlatMovementSpeedMod": 25 }
                    }
                }
                }
                """);

        var service = CreateService(mockHttp);

        var result = await service.GetItemsAsync(version);

        result.Data.Should().ContainKey("1001");
        result.Data["1001"].Name.Should().Be("Boots");
        result.Data["1001"].Gold.Total.Should().Be(300);
        result.Data["1001"].Gold.Sell.Should().Be(210);
        result.Data["1001"].Stats["FlatMovementSpeedMod"].Should().Be(25);
    }

    [Fact]
    public async Task GetItemsAsync_WhenResponseIsNull_Throws()
    {
        const string version = "14.14.1";
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When($"https://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/item.json")
            .Respond("application/json", "null");

        var service = CreateService(mockHttp);

        Func<Task> act = async () => await service.GetItemsAsync(version);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Could not retrieve items data");
    }
}