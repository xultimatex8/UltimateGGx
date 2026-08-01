using AwesomeAssertions;
using backend.Data;
using backend.Interfaces;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace backend.Tests.Services;

public class DataDragonSyncCheckerServiceTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CheckAndSyncAsync_WhenVersionChanged_CallsSyncAndUpdatesState()
    {
        using var db = CreateInMemoryDb();
        db.DataDragonState.Add(new DataDragonState { CurrentVersion = "14.13.1" });
        await db.SaveChangesAsync();

        var mockDdragon = new Mock<IDataDragonService>();
        mockDdragon.Setup(x => x.GetLatestVersionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("14.14.1");
        mockDdragon.Setup(x => x.GetChampionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Models.DataDragon.ChampionResponseDto { Data = [] });
        mockDdragon.Setup(x => x.GetSummonerSpellsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Models.DataDragon.SummonerSpellResponseDto { Data = [] });
        mockDdragon.Setup(x => x.GetItemsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Models.DataDragon.ItemResponseDto { Data = [] });

        var syncService = new ChampionSyncService(mockDdragon.Object, db);
        var checker = new DataDragonSyncCheckerService(
            mockDdragon.Object, syncService, db, NullLogger<DataDragonSyncCheckerService>.Instance);

        await checker.CheckAndSyncAsync();

        var state = await db.DataDragonState.FirstAsync();
        state.CurrentVersion.Should().Be("14.14.1");
        mockDdragon.Verify(x => x.GetChampionsAsync("14.14.1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckAndSyncAsync_WhenVersionUnchanged_DoesNotCallSync()
    {
        using var db = CreateInMemoryDb();
        db.DataDragonState.Add(new DataDragonState { CurrentVersion = "14.14.1" });
        await db.SaveChangesAsync();

        var mockDdragon = new Mock<IDataDragonService>();
        mockDdragon.Setup(x => x.GetLatestVersionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("14.14.1");

        var syncService = new ChampionSyncService(mockDdragon.Object, db);
        var checker = new DataDragonSyncCheckerService(
            mockDdragon.Object, syncService, db, NullLogger<DataDragonSyncCheckerService>.Instance);

        await checker.CheckAndSyncAsync();

        mockDdragon.Verify(x => x.GetChampionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckAndSyncAsync_WhenApiThrows_SwallowsExceptionAndDoesNotCrash()
    {
        using var db = CreateInMemoryDb();

        var mockDdragon = new Mock<IDataDragonService>();
        mockDdragon.Setup(x => x.GetLatestVersionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("boom"));

        var syncService = new ChampionSyncService(mockDdragon.Object, db);
        var checker = new DataDragonSyncCheckerService(
            mockDdragon.Object, syncService, db, NullLogger<DataDragonSyncCheckerService>.Instance);

        Func<Task> act = async () => await checker.CheckAndSyncAsync();

        await act.Should().NotThrowAsync();
    }
}