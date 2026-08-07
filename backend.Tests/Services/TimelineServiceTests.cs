using backend.Data;
using backend.Exceptions;
using backend.Interfaces;
using backend.Models;
using backend.Models.Enums;
using backend.Models.Riot;
using backend.Services;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace backend.Tests.Services;

public class TimelineServiceTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static (Models.Match match, Participant killer, Participant victim, Team teamA) SeedMatchAsync(AppDbContext db)
    {
        var killerSummoner = new Summoner { Puuid = "killer-puuid", Username = "Killer", Tag = "EUW" };
        var victimSummoner = new Summoner { Puuid = "victim-puuid", Username = "Victim", Tag = "EUW" };
        var champion = new Champion { Key = 1, Name = "Annie", RiotId = "Annie", Roles = ["Mage"] };
        var spell = new SummonerSpell { Key = 4, Name = "Flash", RiotId = "SummonerFlash" };
        var primaryRune = new Rune { Key = "PressTheAttack", Name = "Press the Attack", RiotId = 8005, Icon = "perk-images/Styles/Precision/PressTheAttack/PressTheAttack.png", IsStyle = false };
        var secondaryTree = new Rune { Key = "Domination", Name = "Domination", RiotId = 9000, Icon = "perk-images/Styles/Domination/Domination.png", IsStyle = true };

        Team teamA = new() { TeamId = 100, Win = true };
        Team teamB = new() { TeamId = 200, Win = false };

        var killer = new Participant
        {
            ParticipantId = 1,
            Lane = "MIDDLE",
            Team = teamA,
            Summoner = killerSummoner,
            PrimaryRune = primaryRune,
            SecondaryTree = secondaryTree,
            Champion = champion,
            SummonerSpells = [spell]
        };

        var victim = new Participant
        {
            ParticipantId = 2,
            Lane = "JUNGLE",
            Team = teamB,
            PrimaryRune = primaryRune,
            SecondaryTree = secondaryTree,
            Summoner = victimSummoner,
            Champion = champion,
            SummonerSpells = [spell]
        };

        teamA.Participants.Add(killer);
        teamB.Participants.Add(victim);

        var match = new Models.Match
        {
            EndOfGameResult = "GameComplete",
            GameDuration = 1800,
            GameEndTimestamp = 1_700_000_000,
            QueueType = QueueType.DRAFT_PICK,
            Teams = [teamA, teamB]
        };

        var reference = new MatchReference { MatchId = "MATCH_1", QueueType = QueueType.DRAFT_PICK, Match = match };

        db.MatchReferences.Add(reference);
        db.SaveChanges();

        return (match, killer, victim, teamA);
    }

    private static TimelineResponseDto BuildTimeline(params FramesTimeLineDto[] frames)
    {
        return new TimelineResponseDto
        {
            Info = new InfoTimelineDto { Frames = [.. frames] }
        };
    }

    private static EventsTimeLineDto ChampionKillEvent(long timestamp, int killerId, int victimId, int bounty = 300, int shutdownBounty = 0, int[]? assists = null)
    {
        return new EventsTimeLineDto
        {
            Timestamp = timestamp,
            Type = "CHAMPION_KILL",
            KillerId = killerId,
            VictimId = victimId,
            Bounty = bounty,
            ShutdownBounty = shutdownBounty,
            AssistingParticipantIds = assists is not null ? [.. assists] : []
        };
    }

    [Fact]
    public async Task CheckOrFetchTimelineAsync_WhenNoEventsYet_CallsSync()
    {
        using var db = CreateInMemoryDb();
        SeedMatchAsync(db);

        var riotMock = new Mock<IRiotApiService>();
        riotMock.Setup(x => x.GetMatchTimelineAsync("MATCH_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildTimeline());

        var matchMock = new Mock<IMatchService>();

        var service = new TimelineService(db, riotMock.Object, matchMock.Object);

        await service.CheckOrFetchTimelineAsync("MATCH_1");

        riotMock.Verify(x => x.GetMatchTimelineAsync("MATCH_1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckOrFetchTimelineAsync_WhenEventsAlreadyExist_DoesNotCallRiotApi()
    {
        using var db = CreateInMemoryDb();
        var (match, killer, victim, _) = SeedMatchAsync(db);

        db.Events.Add(new Event
        {
            Timestamp = 1000,
            Type = EventType.CHAMPION_KILL,
            Match = match,
            Killer = killer,
            Victim = victim
        });
        await db.SaveChangesAsync();

        var riotMock = new Mock<IRiotApiService>();
        var matchMock = new Mock<IMatchService>();
        var service = new TimelineService(db, riotMock.Object, matchMock.Object);

        await service.CheckOrFetchTimelineAsync("MATCH_1");

        riotMock.Verify(x => x.GetMatchTimelineAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncTimelineAsync_MapsChampionKillEventAndParticipantFrames()
    {
        using var db = CreateInMemoryDb();
        var (match, killer, victim, _) = SeedMatchAsync(db);

        var frame = new FramesTimeLineDto
        {
            Timestamp = 60000,
            Events = [ChampionKillEvent(59000, killerId: 1, victimId: 2, bounty: 300, assists: [])],
            ParticipantFrames = new Dictionary<string, ParticipantFrameDto>
            {
                ["1"] = new ParticipantFrameDto { CurrentGold = 500, TotalGold = 1500, Level = 3, MinionsKilled = 20, JungleMinionsKilled = 2, Position = new PositionDto { X = 100, Y = 200 } },
                ["2"] = new ParticipantFrameDto { CurrentGold = 300, TotalGold = 1200, Level = 3, MinionsKilled = 18, JungleMinionsKilled = 0, Position = new PositionDto { X = 300, Y = 400 } }
            }
        };

        var riotMock = new Mock<IRiotApiService>();
        riotMock.Setup(x => x.GetMatchTimelineAsync("MATCH_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildTimeline(frame));

        var matchMock = new Mock<IMatchService>();

        var service = new TimelineService(db, riotMock.Object, matchMock.Object);

        await service.SyncTimelineAsync("MATCH_1");

        (await db.Events.CountAsync()).Should().Be(1);
        var storedEvent = await db.Events.Include(e => e.Killer).Include(e => e.Victim).SingleAsync();
        storedEvent.Type.Should().Be(EventType.CHAMPION_KILL);
        storedEvent.Killer!.ParticipantId.Should().Be(1);
        storedEvent.Victim!.ParticipantId.Should().Be(2);
        storedEvent.Bounty.Should().Be(300);

        (await db.ParticipantFrames.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task SyncTimelineAsync_SkipsUnknownEventTypes()
    {
        using var db = CreateInMemoryDb();
        SeedMatchAsync(db);

        var frame = new FramesTimeLineDto
        {
            Timestamp = 60000,
            Events = [new EventsTimeLineDto { Timestamp = 1000, Type = "WARD_PLACED" }],
            ParticipantFrames = []
        };

        var riotMock = new Mock<IRiotApiService>();
        riotMock.Setup(x => x.GetMatchTimelineAsync("MATCH_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildTimeline(frame));

        var matchMock = new Mock<IMatchService>();

        var service = new TimelineService(db, riotMock.Object, matchMock.Object);

        await service.SyncTimelineAsync("MATCH_1");

        (await db.Events.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task GetTimelineAsync_RemovesUndoneItemPurchaseAndTheUndoItself()
    {
        using var db = CreateInMemoryDb();
        var (match, killer, _, _) = SeedMatchAsync(db);

        var potion = new Item { Key = 2003, Name = "Health Potion", Description = "d", BuyPrice = 50, SellPrice = 35, Stats = [] };
        db.Items.Add(potion);
        await db.SaveChangesAsync();

        db.Events.Add(new Event { Timestamp = 1800, Type = EventType.ITEM_PURCHASED, Match = match, Participant = killer, Item = potion });
        db.Events.Add(new Event { Timestamp = 1912, Type = EventType.ITEM_UNDO, Match = match, Participant = killer, BeforeItem = potion });
        await db.SaveChangesAsync();

        var riotMock = new Mock<IRiotApiService>();
        var matchMock = new Mock<IMatchService>();
        var service = new TimelineService(db, riotMock.Object, matchMock.Object);

        var timeline = await service.GetTimelineAsync("MATCH_1");

        timeline.Events.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTimelineAsync_RemovesAllDestroyedItemEvents()
    {
        using var db = CreateInMemoryDb();
        var (match, killer, _, _) = SeedMatchAsync(db);

        var component = new Item { Key = 1042, Name = "Dagger", Description = "d", BuyPrice = 300, SellPrice = 210, Stats = [] };
        db.Items.Add(component);
        await db.SaveChangesAsync();

        db.Events.Add(new Event { Timestamp = 1800, Type = EventType.ITEM_DESTROYED, Match = match, Participant = killer, Item = component });
        await db.SaveChangesAsync();

        var riotMock = new Mock<IRiotApiService>();
        var matchMock = new Mock<IMatchService>();
        var service = new TimelineService(db, riotMock.Object, matchMock.Object);

        var timeline = await service.GetTimelineAsync("MATCH_1");

        timeline.Events.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTimelineAsync_KeepsRegularChampionKillEventWithParticipantIds()
    {
        using var db = CreateInMemoryDb();
        var (match, killer, victim, _) = SeedMatchAsync(db);

        db.Events.Add(new Event
        {
            Timestamp = 5000,
            Type = EventType.CHAMPION_KILL,
            Match = match,
            Killer = killer,
            Victim = victim,
            Bounty = 300,
            AssistingParticipants = []
        });
        await db.SaveChangesAsync();

        var riotMock = new Mock<IRiotApiService>();
        var matchMock = new Mock<IMatchService>();
        var service = new TimelineService(db, riotMock.Object, matchMock.Object);

        var timeline = await service.GetTimelineAsync("MATCH_1");

        timeline.Events.Should().ContainSingle();
        var evt = timeline.Events[0];
        evt.Type.Should().Be(EventType.CHAMPION_KILL);
        evt.MainParticipantId.Should().Be(1);
        evt.VictimParticipantId.Should().Be(2);
        evt.Bounty.Should().Be(300);
    }

    [Fact]
    public async Task GetTimelineAsync_SetsWinningTeamIdOnlyOnGameEndEvent()
    {
        using var db = CreateInMemoryDb();
        var (match, killer, victim, teamA) = SeedMatchAsync(db);

        db.Events.Add(new Event { Timestamp = 100, Type = EventType.CHAMPION_KILL, Match = match, Killer = killer, Victim = victim });
        db.Events.Add(new Event { Timestamp = 200000, Type = EventType.GAME_END, Match = match, Team = teamA });
        await db.SaveChangesAsync();

        var riotMock = new Mock<IRiotApiService>();
        var matchMock = new Mock<IMatchService>();
        var service = new TimelineService(db, riotMock.Object, matchMock.Object);

        var timeline = await service.GetTimelineAsync("MATCH_1");

        timeline.Events.Should().HaveCount(2);
        timeline.Events.Single(e => e.Type == EventType.CHAMPION_KILL).TeamId.Should().BeNull();
        timeline.Events.Single(e => e.Type == EventType.GAME_END).TeamId.Should().Be(100);
    }

    [Fact]
    public async Task GetScoreboardAsync_ReturnsGoldAndLevelFromNearestPreviousFrame()
    {
        using var db = CreateInMemoryDb();
        var (match, killer, victim, _) = SeedMatchAsync(db);

        killer.Frames.Add(new ParticipantFrame { Timestamp = 60000, CurrentGold = 500, TotalGold = 1500, Level = 3, Minions = 20, Participant = killer });
        killer.Frames.Add(new ParticipantFrame { Timestamp = 120000, CurrentGold = 900, TotalGold = 2500, Level = 5, Minions = 40, Participant = killer });
        await db.SaveChangesAsync();

        var riotMock = new Mock<IRiotApiService>();
        riotMock.Setup(x => x.GetMatchTimelineAsync("MATCH_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildTimeline());

        var matchMock = new Mock<IMatchService>();

        var service = new TimelineService(db, riotMock.Object, matchMock.Object);

        var scoreboard = await service.GetScoreboardAsync("MATCH_1", timestamp: 90000);

        var killerDto = scoreboard.Teams
            .SelectMany(t => t.Participants)
            .Single(p => p.ParticipantId == 1);

        killerDto.CurrentGold.Should().Be(500);
        killerDto.TotalGold.Should().Be(1500);
        killerDto.ChampionLevel.Should().Be(3);
    }

    [Fact]
    public async Task GetScoreboardAsync_CountsKillsDeathsAssistsOnlyUpToTimestamp()
    {
        using var db = CreateInMemoryDb();
        var (match, killer, victim, _) = SeedMatchAsync(db);

        db.Events.Add(new Event { Timestamp = 60000, Type = EventType.CHAMPION_KILL, Match = match, Killer = killer, Victim = victim });
        db.Events.Add(new Event { Timestamp = 200000, Type = EventType.CHAMPION_KILL, Match = match, Killer = killer, Victim = victim });
        await db.SaveChangesAsync();

        var riotMock = new Mock<IRiotApiService>();
        var matchMock = new Mock<IMatchService>();
        var service = new TimelineService(db, riotMock.Object, matchMock.Object);

        var scoreboard = await service.GetScoreboardAsync("MATCH_1", timestamp: 100000);

        var killerDto = scoreboard.Teams.SelectMany(t => t.Participants).Single(p => p.ParticipantId == 1);
        var victimDto = scoreboard.Teams.SelectMany(t => t.Participants).Single(p => p.ParticipantId == 2);

        killerDto.Kills.Should().Be(1);
        victimDto.Deaths.Should().Be(1);
    }

    [Fact]
    public async Task GetScoreboardAsync_ReconstructsItemsAfterUndo()
    {
        using var db = CreateInMemoryDb();
        var (match, killer, _, _) = SeedMatchAsync(db);

        var boots = new Item { Key = 1001, Name = "Boots", Description = "d", BuyPrice = 300, SellPrice = 210, Stats = [] };
        var potion = new Item { Key = 2003, Name = "Health Potion", Description = "d", BuyPrice = 50, SellPrice = 35, Stats = [] };
        db.Items.AddRange(boots, potion);
        await db.SaveChangesAsync();

        db.Events.Add(new Event { Timestamp = 1000, Type = EventType.ITEM_PURCHASED, Match = match, Participant = killer, Item = boots });
        db.Events.Add(new Event { Timestamp = 1500, Type = EventType.ITEM_PURCHASED, Match = match, Participant = killer, Item = potion });
        db.Events.Add(new Event { Timestamp = 1600, Type = EventType.ITEM_UNDO, Match = match, Participant = killer, BeforeItem = potion });
        await db.SaveChangesAsync();

        var riotMock = new Mock<IRiotApiService>();
        var matchMock = new Mock<IMatchService>();
        var service = new TimelineService(db, riotMock.Object, matchMock.Object);

        var scoreboard = await service.GetScoreboardAsync("MATCH_1", timestamp: 5000);

        var killerDto = scoreboard.Teams.SelectMany(t => t.Participants).Single(p => p.ParticipantId == 1);

        killerDto.Items.Should().ContainSingle();
        killerDto.Items[0].Key.Should().Be(1001);
    }

    [Fact]
    public async Task GetScoreboardAsync_RemovesItemOnSold()
    {
        using var db = CreateInMemoryDb();
        var (match, killer, _, _) = SeedMatchAsync(db);

        var boots = new Item { Key = 1001, Name = "Boots", Description = "d", BuyPrice = 300, SellPrice = 210, Stats = [] };
        db.Items.Add(boots);
        await db.SaveChangesAsync();

        db.Events.Add(new Event { Timestamp = 1000, Type = EventType.ITEM_PURCHASED, Match = match, Participant = killer, Item = boots });
        db.Events.Add(new Event { Timestamp = 2000, Type = EventType.ITEM_SOLD, Match = match, Participant = killer, Item = boots });
        await db.SaveChangesAsync();

        var riotMock = new Mock<IRiotApiService>();
        var matchMock = new Mock<IMatchService>();
        var service = new TimelineService(db, riotMock.Object, matchMock.Object);

        var scoreboard = await service.GetScoreboardAsync("MATCH_1", timestamp: 5000);

        var killerDto = scoreboard.Teams.SelectMany(t => t.Participants).Single(p => p.ParticipantId == 1);

        killerDto.Items.Should().BeEmpty();
    }
}