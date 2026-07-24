using backend.Data;
using backend.Interfaces;
using backend.Models;
using backend.Models.Enums;
using backend.Models.Riot;
using backend.Services;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace backend.Tests.Services;

public class SummonerServiceTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Mock<IRiotApiService> BuildRiotApiMock(
        string puuid = "abc-123",
        string username = "Faker",
        string tag = "KR1",
        int level = 300,
        int profileIconId = 42,
        List<QueueResponseDto>? queues = null)
    {
        var mock = new Mock<IRiotApiService>();
        mock.Setup(x => x.GetRiotAccountAsync(username, tag, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountResponseDto { Puuid = puuid, GameName = username, TagLine = tag });
        mock.Setup(x => x.GetRiotSummonerAsync(puuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SummonerResponseDto { SummonerLevel = level, ProfileIconId = profileIconId });
        mock.Setup(x => x.GetSummonerQueuesAsync(puuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(queues ?? []);
        return mock;
    }

    [Fact]
    public async Task GetOrFetchSummonerAsync_WhenNotInDb_FetchesFromRiotAndPersists()
    {
        using var db = CreateInMemoryDb();
        var riotMock = BuildRiotApiMock(queues:
        [
            new() { QueueType = "RANKED_SOLO_5x5", Tier = "GOLD", Rank = "II", LeaguePoints = 55, Wins = 10, Losses = 5 }
        ]);

        var service = new SummonerService(db, riotMock.Object);

        var dto = await service.GetOrFetchSummonerAsync("Faker", "KR1");

        dto.Username.Should().Be("Faker");
        dto.Level.Should().Be(300);
        dto.Queues.Should().ContainSingle(q => q.Type == QueueType.RANKED_SOLO);

        (await db.Summoners.CountAsync()).Should().Be(1);
        riotMock.Verify(x => x.GetRiotAccountAsync("Faker", "KR1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrFetchSummonerAsync_WhenAlreadyInDb_DoesNotCallRiotApi()
    {
        using var db = CreateInMemoryDb();
        db.Summoners.Add(new Summoner
        {
            Puuid = "abc-123",
            Username = "Faker",
            Tag = "KR1",
            Level = 300,
            ProfileIconId = 42,
            Queues = []
        });
        await db.SaveChangesAsync();

        var riotMock = BuildRiotApiMock();
        var service = new SummonerService(db, riotMock.Object);

        var dto = await service.GetOrFetchSummonerAsync("Faker", "KR1");

        dto.Username.Should().Be("Faker");
        riotMock.Verify(x => x.GetRiotAccountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncSummonerAsync_WhenResyncing_ReplacesOldQueuesInsteadOfDuplicating()
    {
        using var db = CreateInMemoryDb();
        var summoner = new Summoner
        {
            Puuid = "abc-123",
            Username = "Faker",
            Tag = "KR1",
            Queues =
            [
                new() { Type = QueueType.RANKED_SOLO, Tier = "SILVER", Rank = "I", Points = 10, Wins = 1, Losses = 1 }
            ]
        };
        db.Summoners.Add(summoner);
        await db.SaveChangesAsync();

        var riotMock = BuildRiotApiMock(queues:
        [
            new() { QueueType = "RANKED_SOLO_5x5", Tier = "GOLD", Rank = "II", LeaguePoints = 55, Wins = 10, Losses = 5 }
        ]);
        var service = new SummonerService(db, riotMock.Object);

        var dto = await service.SyncSummonerAsync("Faker", "KR1");

        dto.Queues.Should().ContainSingle();
        dto.Queues[0].Tier.Should().Be("GOLD");

        var queuesInDb = await db.Queues.ToListAsync();
        queuesInDb.Should().ContainSingle();
        queuesInDb[0].Tier.Should().Be("GOLD");
    }

    [Fact]
    public async Task SyncSummonerAsync_WithUnknownQueueType_ThrowsArgumentOutOfRangeException()
    {
        using var db = CreateInMemoryDb();
        var riotMock = BuildRiotApiMock(queues:
        [
            new() { QueueType = "RANKED_TFT_TURBO", Tier = "GOLD", Rank = "II", LeaguePoints = 0, Wins = 0, Losses = 0 }
        ]);
        var service = new SummonerService(db, riotMock.Object);

        Func<Task> act = async () => await service.SyncSummonerAsync("Faker", "KR1");

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData("RANKED_SOLO_5x5", QueueType.RANKED_SOLO)]
    [InlineData("RANKED_FLEX_SR", QueueType.RANKED_FLEX)]
    public async Task SyncSummonerAsync_MapsKnownQueueTypesCorrectly(string riotQueueType, QueueType expected)
    {
        using var db = CreateInMemoryDb();
        var riotMock = BuildRiotApiMock(queues:
        [
            new() { QueueType = riotQueueType, Tier = "GOLD", Rank = "II", LeaguePoints = 0, Wins = 0, Losses = 0 }
        ]);
        var service = new SummonerService(db, riotMock.Object);

        var dto = await service.SyncSummonerAsync("Faker", "KR1");

        dto.Queues.Single().Type.Should().Be(expected);
    }
}