using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataDragonController : ControllerBase
{
    private readonly AppDbContext _db;
    public DataDragonController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("version")]
    public async Task<ActionResult> GetCurrentVersion(CancellationToken ct)
    {
        string version = await _db.DataDragonState
            .Select(dd => dd.CurrentVersion)
            .FirstAsync(ct);

        return Ok(new { version });
    }
}