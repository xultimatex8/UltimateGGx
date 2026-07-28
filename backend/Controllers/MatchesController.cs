using backend.Interfaces;
using backend.Models.Dtos;
using backend.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly IMatchService _matchService;

    public MatchesController(IMatchService matchService)
    {
        _matchService = matchService;
    }

    [HttpPost("summoner/{puuid}")]
    public async Task<IActionResult> FetchSummonerMatches(
        string puuid,
        [FromQuery] QueueType queueType = QueueType.DRAFT_PICK)
    {
        await _matchService.FetchSummonerMatchesAsync(puuid, queueType);
        return Ok();
    }

    [HttpGet("summoner/{puuid}")]
    public async Task<IActionResult> GetSummonerMatches(
        string puuid,
        [FromQuery] QueueType queueType = QueueType.DRAFT_PICK)
    {
        List<MatchDto> matches = await _matchService.GetSummonerMatchesAsync(puuid, queueType);
        return Ok(matches);
    }
}