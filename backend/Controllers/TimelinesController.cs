using backend.Interfaces;
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
    public async Task<IActionResult> FetchSummonerMatches(string matchId)
    {
        await _timelineService.GetOrFetchTimelineAsync(matchId);
        return Ok();
    }
}