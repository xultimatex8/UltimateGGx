using backend.Controllers;
using backend.Interfaces;
using backend.Models.Dtos;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace backend.Tests.Controllers;

public class SummonersControllerTests
{
    [Fact]
    public async Task GetSummoner_ReturnsOkWithSummonerDto()
    {
        var mockService = new Mock<ISummonerService>();
        mockService.Setup(x => x.GetOrFetchSummonerAsync("Faker", "KR1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SummonerDto { Username = "Faker", Tag = "KR1", Level = 300 });

        var controller = new SummonersController(mockService.Object);

        var result = await controller.GetSummoner("Faker", "KR1");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<SummonerDto>().Subject;
        dto.Username.Should().Be("Faker");
    }

    [Fact]
    public async Task RefreshSummoner_CallsSyncAndReturnsOk()
    {
        var mockService = new Mock<ISummonerService>();
        mockService.Setup(x => x.SyncSummonerAsync("Faker", "KR1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SummonerDto { Username = "Faker", Tag = "KR1", Level = 301 });

        var controller = new SummonersController(mockService.Object);

        var result = await controller.RefreshSummoner("Faker", "KR1");

        result.Should().BeOfType<OkObjectResult>();
        mockService.Verify(x => x.SyncSummonerAsync("Faker", "KR1", It.IsAny<CancellationToken>()), Times.Once);
    }
}