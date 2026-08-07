using backend.Controllers;
using backend.Interfaces;
using backend.Models.Dtos;
using backend.Models.Enums;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace backend.Tests.Controllers;

public class MatchesControllerTests
{
    [Fact]
    public async Task FetchSummonerMatches_CallsServiceAndReturnsOk()
    {
        var mockService = new Mock<IMatchService>();
        mockService.Setup(x => x.FetchSummonerMatchesAsync("abc-123", QueueType.DRAFT_PICK, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = new MatchesController(mockService.Object);

        var result = await controller.FetchSummonerMatches("abc-123");

        result.Should().BeOfType<OkResult>();
        mockService.Verify(x => x.FetchSummonerMatchesAsync("abc-123", QueueType.DRAFT_PICK, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FetchSummonerMatches_WithExplicitQueueType_PassesItThrough()
    {
        var mockService = new Mock<IMatchService>();
        mockService.Setup(x => x.FetchSummonerMatchesAsync("abc-123", QueueType.RANKED_SOLO, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = new MatchesController(mockService.Object);

        await controller.FetchSummonerMatches("abc-123", QueueType.RANKED_SOLO);

        mockService.Verify(x => x.FetchSummonerMatchesAsync("abc-123", QueueType.RANKED_SOLO, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSummonerMatches_ReturnsOkWithPagedResult()
    {
        var pagedResult = new PagedResult<MatchDto>
        {
            Items = [new MatchDto { EndOfGameResult = "GameComplete" }],
            Page = 1,
            PageSize = 10,
            TotalItems = 1
        };

        var mockService = new Mock<IMatchService>();
        mockService.Setup(x => x.GetSummonerMatchesAsync(
                "abc-123", 1, 10, QueueType.DRAFT_PICK, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var controller = new MatchesController(mockService.Object);

        var result = await controller.GetSummonerMatches("abc-123");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<PagedResult<MatchDto>>().Subject;
        dto.TotalItems.Should().Be(1);
        dto.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task GetSummonerMatches_WithCustomPagingAndQueueType_PassesArgumentsThrough()
    {
        var mockService = new Mock<IMatchService>();
        mockService.Setup(x => x.GetSummonerMatchesAsync(
                "abc-123", 2, 5, QueueType.RANKED_FLEX, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<MatchDto> { Items = [], Page = 2, PageSize = 5, TotalItems = 0 });

        var controller = new MatchesController(mockService.Object);

        await controller.GetSummonerMatches("abc-123", QueueType.RANKED_FLEX, page: 2, pageSize: 5);

        mockService.Verify(x => x.GetSummonerMatchesAsync(
            "abc-123", 2, 5, QueueType.RANKED_FLEX, It.IsAny<CancellationToken>()), Times.Once);
    }
}