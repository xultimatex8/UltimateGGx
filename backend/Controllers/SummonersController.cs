using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SummonersController : ControllerBase
{
    private readonly SummonerService _summonerService;

    public SummonersController(SummonerService summonerService)
    {
        _summonerService = summonerService;
    }

    [HttpGet("{username}/{tag}")]
    public async Task<IActionResult> GetSummoner(string username, string tag)
    {
        var summoner = await _summonerService.GetOrFetchSummonerAsync(username, tag);
        return Ok(summoner);
    }
}