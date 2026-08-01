using backend.Data;
using backend.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Models.Enums;
using backend.Models.Riot;
using backend.Services;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using backend.Exceptions;

namespace backend.Tests.Services;

public class MatchServiceTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task SeedReferenceDataAsync(AppDbContext db)
    {
        db.Champions.Add(new Champion { Key = 1, Name = "Annie", Roles = ["Mage"] });
        db.SummonerSpells.Add(new SummonerSpell { Key = 4, Name = "Flash" });
        db.SummonerSpells.Add(new SummonerSpell { Key = 7, Name = "Heal" });
        db.Items.Add(new Item { Key = 1001, Name = "Boots", Description = "desc", BuyPrice = 300, SellPrice = 210, Stats = [] });
        db.Items.Add(new Item { Key = 3006, Name = "Berserker's Greaves", Description = "desc", BuyPrice = 1100, SellPrice = 770, Stats = [] });
        await db.SaveChangesAsync();
    }

    private static ParticipantDto BuildParticipantDto(
        string puuid,
        int teamId,
        int participantId,
        string username = "SomePlayer",
        string tag = "EUW",
        int[]? itemIds = null)
    {
        itemIds ??= [0, 0, 0, 0, 0, 0, 0];

        return new ParticipantDto
        {
            Puuid = puuid,
            ParticipantId = participantId,
            TeamId = teamId,
            ChampionId = 1,
            ChampLevel = 15,
            Assists = 5,
            Deaths = 2,
            Kills = 8,
            GoldEarned = 12000,
            TotalDamageDealtToChampions = 20000,
            TotalMinionsKilled = 150,
            NeutralMinionsKilled = 10,
            TeamPosition = "MIDDLE",
            Summoner1Id = 4,
            Summoner2Id = 7,
            Item0 = itemIds[0],
            Item1 = itemIds[1],
            Item2 = itemIds[2],
            Item3 = itemIds[3],
            Item4 = itemIds[4],
            Item5 = itemIds[5],
            Item6 = itemIds[6],
            RiotIdGameName = username,
            RiotIdTagLine = tag,
            SummonerLevel = 200,
            ProfileIcon = 10,
            Perks = new PerksDto
            {
                Styles =
                [
                    new PerkStyleDto { Style = 8000, Selections = [new PerkStyleSelectionDto { Perk = 8005 }] },
                    new PerkStyleDto { Style = 8100, Selections = [new PerkStyleSelectionDto { Perk = 8100 }] }
                ]
            }
        };
    }

    private static MatchResponseDto BuildMatchResponseDto(string requestedPuuid)
    {
        return new MatchResponseDto
        {
            Info = new InfoDto
            {
                EndOfGameResult = "GameComplete",
                GameDuration = 1800,
                GameEndTimestamp = 1_700_000_000,
                QueueId = 400,
                Teams =
                [
                    new TeamDto { TeamId = 100, Win = true },
                    new TeamDto { TeamId = 200, Win = false }
                ],
                Participants =
                [
                    BuildParticipantDto(requestedPuuid, 100, 1, "Requested", "EUW1", [1001, 3006, 0, 0, 0, 0, 0]),
                    BuildParticipantDto("rival-puuid", 200, 2, "Rival", "EUW2")
                ]
            }
        };
    }

    [Fact]
    public async Task FetchSummonerMatchesAsync_WhenSummonerNotFound_ThrowsNotFoundException()
    {
        using var db = CreateInMemoryDb();
        var riotMock = new Mock<IRiotApiService>();
        riotMock.Setup(x => x.GetSummonerMatchesAsync("abc-123", QueueType.DRAFT_PICK, It.IsAny<CancellationToken>()))
            .ReturnsAsync(["MATCH_1"]);

        var service = new MatchService(db, riotMock.Object);

        Func<Task> act = async () => await service.FetchSummonerMatchesAsync("abc-123");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task FetchSummonerMatchesAsync_CreatesNewMatchReferencesAndLinksToSummoner()
    {
        using var db = CreateInMemoryDb();
        db.Summoners.Add(new Summoner { Puuid = "abc-123", Username = "Faker", Tag = "KR1" });
        await db.SaveChangesAsync();

        var riotMock = new Mock<IRiotApiService>();
        riotMock.Setup(x => x.GetSummonerMatchesAsync("abc-123", QueueType.DRAFT_PICK, It.IsAny<CancellationToken>()))
            .ReturnsAsync(["MATCH_1", "MATCH_2"]);

        var service = new MatchService(db, riotMock.Object);

        await service.FetchSummonerMatchesAsync("abc-123");

        (await db.MatchReferences.CountAsync()).Should().Be(2);

        var summoner = await db.Summoners
            .Include(s => s.MatchReferences)
            .FirstAsync(s => s.Puuid == "abc-123");

        summoner.MatchReferences.Should().HaveCount(2);
        summoner.MatchReferences.Select(m => m.MatchId).Should().BeEquivalentTo(["MATCH_1", "MATCH_2"]);
    }

    [Fact]
    public async Task FetchSummonerMatchesAsync_WhenMatchReferenceAlreadyExists_ReusesItInsteadOfDuplicating()
    {
        using var db = CreateInMemoryDb();
        db.Summoners.Add(new Summoner { Puuid = "abc-123", Username = "Faker", Tag = "KR1" });
        db.Summoners.Add(new Summoner { Puuid = "other-puuid", Username = "Other", Tag = "EUW" });
        await db.SaveChangesAsync();

        var otherSummoner = await db.Summoners.FirstAsync(s => s.Puuid == "other-puuid");
        var existingReference = new MatchReference { MatchId = "MATCH_1", QueueType = QueueType.DRAFT_PICK };
        existingReference.Summoners.Add(otherSummoner);
        db.MatchReferences.Add(existingReference);
        await db.SaveChangesAsync();

        var riotMock = new Mock<IRiotApiService>();
        riotMock.Setup(x => x.GetSummonerMatchesAsync("abc-123", QueueType.DRAFT_PICK, It.IsAny<CancellationToken>()))
            .ReturnsAsync(["MATCH_1"]);

        var service = new MatchService(db, riotMock.Object);

        await service.FetchSummonerMatchesAsync("abc-123");

        (await db.MatchReferences.CountAsync()).Should().Be(1);

        var reference = await db.MatchReferences
            .Include(m => m.Summoners)
            .FirstAsync(m => m.MatchId == "MATCH_1");

        reference.Summoners.Select(s => s.Puuid).Should().BeEquivalentTo(["abc-123", "other-puuid"]);
    }

    [Fact]
    public async Task GetSummonerMatchesAsync_WhenMatchAlreadySynced_DoesNotCallRiotApiAndMapsCorrectly()
    {
        using var db = CreateInMemoryDb();
        await SeedReferenceDataAsync(db);

        var requestedSummoner = new Summoner { Puuid = "abc-123", Username = "Faker", Tag = "KR1" };
        var rivalSummoner = new Summoner { Puuid = "rival-puuid", Username = "Rival", Tag = "EUW" };
        db.Summoners.AddRange(requestedSummoner, rivalSummoner);
        await db.SaveChangesAsync();

        Team teamA = new() { TeamId = 100, Win = true };
        Team teamB = new() { TeamId = 200, Win = false };

        SummonerSpell flash = await db.SummonerSpells.FirstAsync(s => s.Key == 4);
        SummonerSpell heal = await db.SummonerSpells.FirstAsync(s => s.Key == 7);
        Champion champion = await db.Champions.FirstAsync(c => c.Key == 1);

        teamA.Participants.Add(new Participant
        {
            ParticipantId = 1,
            Kills = 10,
            Deaths = 1,
            Assists = 5,
            Gold = 15000,
            Items = [],
            Lane = "MIDDLE",
            PrimaryRune = 8005,
            SecondaryTree = 8100,
            DamageToChampions = 25000,
            Team = teamA,
            Summoner = requestedSummoner,
            Champion = champion,
            SummonerSpells = [flash, heal]
        });

        var match = new Models.Match
        {
            EndOfGameResult = "GameComplete",
            GameDuration = 1800,
            GameEndTimestamp = 1_700_000_000,
            QueueType = QueueType.DRAFT_PICK,
            Teams = [teamA, teamB]
        };

        var reference = new MatchReference { MatchId = "MATCH_1", QueueType = QueueType.DRAFT_PICK, Match = match };
        reference.Summoners.Add(requestedSummoner);
        db.MatchReferences.Add(reference);
        await db.SaveChangesAsync();

        var riotMock = new Mock<IRiotApiService>();
        var service = new MatchService(db, riotMock.Object);

        var result = await service.GetSummonerMatchesAsync("abc-123");

        result.TotalItems.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].EndOfGameResult.Should().Be("GameComplete");
        result.Items[0].Win.Should().BeTrue();

        riotMock.Verify(x => x.GetMatchDetailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSummonerMatchesAsync_WhenMatchNotYetSynced_MapsParticipantItems()
    {
        using var db = CreateInMemoryDb();
        await SeedReferenceDataAsync(db);

        var requestedSummoner = new Summoner { Puuid = "abc-123", Username = "Faker", Tag = "KR1" };
        db.Summoners.Add(requestedSummoner);
        await db.SaveChangesAsync();

        var reference = new MatchReference { MatchId = "MATCH_1", QueueType = QueueType.DRAFT_PICK };
        reference.Summoners.Add(requestedSummoner);
        db.MatchReferences.Add(reference);
        await db.SaveChangesAsync();

        var riotMock = new Mock<IRiotApiService>();
        riotMock.Setup(x => x.GetMatchDetailAsync("MATCH_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildMatchResponseDto("abc-123"));

        var service = new MatchService(db, riotMock.Object);

        var result = await service.GetSummonerMatchesAsync("abc-123");

        var requestedParticipant = result.Items[0].Participants.Single(p => p.TeamId == 100);
        requestedParticipant.Items.Should().HaveCount(2);
        requestedParticipant.Items.Select(i => i.Key).Should().BeEquivalentTo([1001, 3006]);
        requestedParticipant.Items.Single(i => i.Key == 1001).Name.Should().Be("Boots");
    }

    [Fact]
    public async Task GetSummonerMatchesAsync_WhenChampionUnknown_ThrowsNotFoundException()
    {
        using var db = CreateInMemoryDb();

        var requestedSummoner = new Summoner { Puuid = "abc-123", Username = "Faker", Tag = "KR1" };
        db.Summoners.Add(requestedSummoner);

        var reference = new MatchReference { MatchId = "MATCH_1", QueueType = QueueType.DRAFT_PICK };
        reference.Summoners.Add(requestedSummoner);
        db.MatchReferences.Add(reference);
        await db.SaveChangesAsync();

        var riotMock = new Mock<IRiotApiService>();
        riotMock.Setup(x => x.GetMatchDetailAsync("MATCH_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildMatchResponseDto("abc-123"));

        var service = new MatchService(db, riotMock.Object);

        Func<Task> act = async () => await service.GetSummonerMatchesAsync("abc-123");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetSummonerMatchesAsync_RespectsPagingParameters()
    {
        using var db = CreateInMemoryDb();
        await SeedReferenceDataAsync(db);

        var requestedSummoner = new Summoner { Puuid = "abc-123", Username = "Faker", Tag = "KR1" };
        db.Summoners.Add(requestedSummoner);
        await db.SaveChangesAsync();

        for (int i = 1; i <= 15; i++)
        {
            var reference = new MatchReference { MatchId = $"MATCH_{i}", QueueType = QueueType.DRAFT_PICK };
            reference.Summoners.Add(requestedSummoner);
            db.MatchReferences.Add(reference);
        }
        await db.SaveChangesAsync();

        var riotMock = new Mock<IRiotApiService>();
        riotMock.Setup(x => x.GetMatchDetailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string matchId, CancellationToken _) => BuildMatchResponseDto("abc-123"));

        var service = new MatchService(db, riotMock.Object);

        var result = await service.GetSummonerMatchesAsync("abc-123", page: 2, pageSize: 5);

        result.TotalItems.Should().Be(15);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetSummonerMatchesAsync_WhenItemUnknown_ThrowsNotFoundException()
    {
        using var db = CreateInMemoryDb();
        db.Champions.Add(new Champion { Key = 1, Name = "Annie", Roles = ["Mage"] });
        db.SummonerSpells.Add(new SummonerSpell { Key = 4, Name = "Flash" });
        db.SummonerSpells.Add(new SummonerSpell { Key = 7, Name = "Heal" });
        await db.SaveChangesAsync();

        var requestedSummoner = new Summoner { Puuid = "abc-123", Username = "Faker", Tag = "KR1" };
        db.Summoners.Add(requestedSummoner);
        await db.SaveChangesAsync();

        var reference = new MatchReference { MatchId = "MATCH_1", QueueType = QueueType.DRAFT_PICK };
        reference.Summoners.Add(requestedSummoner);
        db.MatchReferences.Add(reference);
        await db.SaveChangesAsync();

        var riotMock = new Mock<IRiotApiService>();
        riotMock.Setup(x => x.GetMatchDetailAsync("MATCH_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildMatchResponseDto("abc-123"));

        var service = new MatchService(db, riotMock.Object);

        Func<Task> act = async () => await service.GetSummonerMatchesAsync("abc-123");

        await act.Should().ThrowAsync<NotFoundException>();
    }
}