using backend.Controllers;
using backend.Interfaces;
using backend.Models.Dtos;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace backend.Tests.Controllers;

public class TimelinesControllerTests
{
    [Fact]
    public async Task GetMatchTimeline_ReturnsOkWithTimelineDto()
    {
        var timelineMock = new Mock<ITimelineService>();
        var expected = new TimelineDto { Events = [] };
        timelineMock.Setup(x => x.GetTimelineAsync("MATCH_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new TimelinesController(timelineMock.Object);

        var result = await controller.GetMatchTimeline("MATCH_1");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetMatchScoreboard_PassesTimestampAndReturnsOkWithScoreboardDto()
    {
        var timelineMock = new Mock<ITimelineService>();
        var expected = new ScoreboardDto { Timestamp = 90000, Teams = [] };
        timelineMock.Setup(x => x.GetScoreboardAsync("MATCH_1", 90000, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new TimelinesController(timelineMock.Object);

        var result = await controller.GetMatchScoreboard("MATCH_1", 90000);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeSameAs(expected);

        timelineMock.Verify(x => x.GetScoreboardAsync("MATCH_1", 90000, It.IsAny<CancellationToken>()), Times.Once);
    }
}