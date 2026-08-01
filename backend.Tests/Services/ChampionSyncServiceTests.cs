using backend.Data;
using backend.Interfaces;
using backend.Models;
using backend.Models.DataDragon;
using backend.Services;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace backend.Tests.Services;

public class ChampionSyncServiceTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static ChampionResponseDto BuildChampionsResponse(params (string key, string name, string id, List<string> tags)[] champs)
    {
        var data = champs.ToDictionary(
            c => c.name,
            c => new ChampionDto { Key = c.key, Name = c.name, Id = c.id, Tags = c.tags });

        return new ChampionResponseDto { Data = data };
    }

    private static SummonerSpellResponseDto BuildSpellsResponse(params (string key, string name, string id)[] spells)
    {
        var data = spells.ToDictionary(
            s => s.name,
            s => new SummonerSpellDto { Key = s.key, Name = s.name, Id = s.id });

        return new SummonerSpellResponseDto { Data = data };
    }

    private static ItemResponseDto BuildItemsResponse(params (string key, string name, int total, int sell)[] items)
    {
        var data = items.ToDictionary(
            i => i.key,
            i => new ItemDto
            {
                Name = i.name,
                Description = "<stats>+10 AD</stats><mainText>Basic item.</mainText>",
                Gold = new ItemGoldDto { Total = i.total, Sell = i.sell },
                Stats = new Dictionary<string, double> { ["FlatPhysicalDamageMod"] = 10 }
            });

        return new ItemResponseDto { Data = data };
    }

    [Fact]
    public async Task SyncAsync_InsertsNewChampionsAndSpells()
    {
        using var db = CreateInMemoryDb();

        var mockDdragon = new Mock<IDataDragonService>();
        mockDdragon.Setup(x => x.GetLatestVersionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("14.14.1");
        mockDdragon.Setup(x => x.GetChampionsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildChampionsResponse(("103", "Ahri", "Ahri", ["Mage", "Assassin"])));
        mockDdragon.Setup(x => x.GetSummonerSpellsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildSpellsResponse(("4", "Flash", "SummonerFlash")));
        mockDdragon.Setup(x => x.GetItemsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildItemsResponse());

        var syncService = new ChampionSyncService(mockDdragon.Object, db);

        await syncService.SyncAsync();

        var champion = await db.Champions.SingleAsync();
        champion.Key.Should().Be(103);
        champion.Name.Should().Be("Ahri");
        champion.RiotId.Should().Be("Ahri");
        champion.Roles.Should().BeEquivalentTo(["Mage", "Assassin"]);

        var spell = await db.SummonerSpells.SingleAsync();
        spell.Key.Should().Be(4);
        spell.Name.Should().Be("Flash");
        spell.RiotId.Should().Be("SummonerFlash");
    }

    [Fact]
    public async Task SyncAsync_UpdatesExistingChampionInsteadOfDuplicating()
    {
        using var db = CreateInMemoryDb();
        db.Champions.Add(new Champion { Key = 103, Name = "Ahri (old name)", RiotId = "Ahri", Roles = ["Mage"] });
        await db.SaveChangesAsync();

        var mockDdragon = new Mock<IDataDragonService>();
        mockDdragon.Setup(x => x.GetLatestVersionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("14.14.1");
        mockDdragon.Setup(x => x.GetChampionsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildChampionsResponse(("103", "Ahri", "Ahri", ["Mage", "Assassin"])));
        mockDdragon.Setup(x => x.GetSummonerSpellsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildSpellsResponse(("4", "Flash", "SummonerFlash")));
        mockDdragon.Setup(x => x.GetItemsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildItemsResponse());

        var syncService = new ChampionSyncService(mockDdragon.Object, db);

        await syncService.SyncAsync();

        (await db.Champions.CountAsync()).Should().Be(1);
        var champion = await db.Champions.SingleAsync();
        champion.Name.Should().Be("Ahri");
        champion.Roles.Should().BeEquivalentTo(["Mage", "Assassin"]);
    }

    [Fact]
    public async Task SyncAsync_UpdatesExistingSpellInsteadOfDuplicating()
    {
        using var db = CreateInMemoryDb();
        db.SummonerSpells.Add(new SummonerSpell { Key = 4, Name = "Old Flash Name", RiotId = "SummonerFlash" });
        await db.SaveChangesAsync();

        var mockDdragon = new Mock<IDataDragonService>();
        mockDdragon.Setup(x => x.GetLatestVersionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("14.14.1");
        mockDdragon.Setup(x => x.GetChampionsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildChampionsResponse());
        mockDdragon.Setup(x => x.GetSummonerSpellsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildSpellsResponse(("4", "Flash", "SummonerFlash")));
        mockDdragon.Setup(x => x.GetItemsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildItemsResponse());

        var syncService = new ChampionSyncService(mockDdragon.Object, db);

        await syncService.SyncAsync();

        (await db.SummonerSpells.CountAsync()).Should().Be(1);
        (await db.SummonerSpells.SingleAsync()).Name.Should().Be("Flash");
    }

    [Fact]
    public async Task SyncAsync_WithNoChampionsOrSpellsOrItems_DoesNotThrow()
    {
        using var db = CreateInMemoryDb();

        var mockDdragon = new Mock<IDataDragonService>();
        mockDdragon.Setup(x => x.GetLatestVersionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("14.14.1");
        mockDdragon.Setup(x => x.GetChampionsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildChampionsResponse());
        mockDdragon.Setup(x => x.GetSummonerSpellsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildSpellsResponse());
        mockDdragon.Setup(x => x.GetItemsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildItemsResponse());

        var syncService = new ChampionSyncService(mockDdragon.Object, db);

        Func<Task> act = async () => await syncService.SyncAsync();

        await act.Should().NotThrowAsync();
        (await db.Champions.CountAsync()).Should().Be(0);
        (await db.SummonerSpells.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task SyncAsync_InsertsNewItems()
    {
        using var db = CreateInMemoryDb();

        var mockDdragon = new Mock<IDataDragonService>();
        mockDdragon.Setup(x => x.GetLatestVersionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("14.14.1");
        mockDdragon.Setup(x => x.GetChampionsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildChampionsResponse());
        mockDdragon.Setup(x => x.GetSummonerSpellsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildSpellsResponse());
        mockDdragon.Setup(x => x.GetItemsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildItemsResponse(("1001", "Boots", 300, 210)));

        var syncService = new ChampionSyncService(mockDdragon.Object, db);

        await syncService.SyncAsync();

        var item = await db.Items.SingleAsync();
        item.Key.Should().Be(1001);
        item.Name.Should().Be("Boots");
        item.BuyPrice.Should().Be(300);
        item.SellPrice.Should().Be(210);
        item.Stats.Should().ContainKey("FlatPhysicalDamageMod");
        item.Description.Should().NotContain("<stats>");
        item.Description.Should().NotContain("<mainText>");
        item.Description.Should().Contain("Basic item.");
    }

    [Fact]
    public async Task SyncAsync_UpdatesExistingItemInsteadOfDuplicating()
    {
        using var db = CreateInMemoryDb();
        db.Items.Add(new Item
        {
            Key = 1001,
            Name = "Boots (old)",
            Description = "old",
            BuyPrice = 250,
            SellPrice = 175,
            Stats = []
        });
        await db.SaveChangesAsync();

        var mockDdragon = new Mock<IDataDragonService>();
        mockDdragon.Setup(x => x.GetLatestVersionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("14.14.1");
        mockDdragon.Setup(x => x.GetChampionsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildChampionsResponse());
        mockDdragon.Setup(x => x.GetSummonerSpellsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildSpellsResponse());
        mockDdragon.Setup(x => x.GetItemsAsync("14.14.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildItemsResponse(("1001", "Boots", 300, 210)));

        var syncService = new ChampionSyncService(mockDdragon.Object, db);

        await syncService.SyncAsync();

        (await db.Items.CountAsync()).Should().Be(1);
        var item = await db.Items.SingleAsync();
        item.Name.Should().Be("Boots");
        item.BuyPrice.Should().Be(300);
    }
}