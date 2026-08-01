using System.Reflection;
using AwesomeAssertions;
using backend.Controllers;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Tests.Controllers;

public class DataDragonControllerTests
{
    [Fact]
    public async Task GetCurrentVersion_ReturnsOkWithVersion()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new AppDbContext(options);

        db.DataDragonState.Add(new DataDragonState
        {
            CurrentVersion = "15.14.1"
        });

        await db.SaveChangesAsync();

        var controller = new DataDragonController(db);

        var result = await controller.GetCurrentVersion(CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

        var version = okResult.Value!
            .GetType()
            .GetProperty("version", BindingFlags.Public | BindingFlags.Instance)!
            .GetValue(okResult.Value);

        version.Should().Be("15.14.1");
    }
}