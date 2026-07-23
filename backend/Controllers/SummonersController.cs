using backend.Interfaces;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SummonersController : ControllerBase
{
    private readonly ISummonerService _summonerService;

    public SummonersController(ISummonerService summonerService)
    {
        _summonerService = summonerService;
    }

    [HttpGet("{username}/{tag}")]
    public async Task<IActionResult> GetSummoner(string username, string tag)
    {
        var summoner = await _summonerService.GetOrFetchSummonerAsync(username, tag);
        return Ok(summoner);
    }

    [HttpPut("{username}/{tag}/refresh")]
    public async Task<IActionResult> RefreshSummoner(string username, string tag)
    {
        var summoner = await _summonerService.SyncSummonerAsync(username, tag);
        return Ok(summoner);
    }
}