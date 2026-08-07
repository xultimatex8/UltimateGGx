using backend.Interfaces;
using backend.Models.Dtos;
using backend.Models.Riot;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimelinesController : ControllerBase
{
    private readonly ITimelineService _timelineService;

    public TimelinesController(ITimelineService timelineService)
    {
        _timelineService = timelineService;
    }

    [HttpGet("match/{matchId}")]
    public async Task<IActionResult> GetMatchTimeline(string matchId)
    {
        TimelineDto timeline = await _timelineService.GetTimelineAsync(matchId);
        return Ok(timeline);
    }

    [HttpGet("match/{matchId}/scoreboard")]
    public async Task<IActionResult> GetMatchScoreboard(string matchId, [FromQuery] long timestamp)
    {
        ScoreboardDto scoreboard = await _timelineService.GetScoreboardAsync(matchId, timestamp);
        return Ok(scoreboard);
    }
}