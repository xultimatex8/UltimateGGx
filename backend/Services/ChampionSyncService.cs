using backend.Data;
using backend.Models;
using backend.Models.DataDragon;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ChampionSyncService
{
    private readonly DataDragonService _dataDragonService;
    private readonly AppDbContext _db;

    public ChampionSyncService(DataDragonService dataDragonService, AppDbContext db)
    {
        _dataDragonService = dataDragonService;
        _db = db;
    }

    public async Task SyncAsync(CancellationToken ct = default)
    {
        string version = await _dataDragonService.GetLatestVersionAsync(ct);

        ChampionResponseDto championsResponse = await _dataDragonService.GetChampionsAsync(version, ct);
        foreach (var (_, championDto) in championsResponse.Data)
        {
            int key = int.Parse(championDto.Key);
            Champion? existing = await _db.Champions
                .FirstOrDefaultAsync(c => c.Key == key, ct);

            if (existing is null)
            {
                _db.Champions.Add(new Champion
                {
                    Key = key,
                    Name = championDto.Name,
                    Roles = championDto.Tags
                });
            }
            else
            {
                existing.Name = championDto.Name;
                existing.Roles = championDto.Tags;
            }
        }

        SummonerSpellResponseDto spellsResponse = await _dataDragonService.GetSummonerSpellsAsync(version, ct);
        foreach (var (_, spellDto) in spellsResponse.Data)
        {
            int key = int.Parse(spellDto.Key);
            SummonerSpell? existing = await _db.SummonerSpells
                .FirstOrDefaultAsync(sp => sp.Key == key, ct);

            if (existing is null)
            {
                _db.SummonerSpells.Add(new SummonerSpell
                {
                    Key = key,
                    Name = spellDto.Name
                });
            }
            else
            {
                existing.Name = spellDto.Name;
            }
        }

        await _db.SaveChangesAsync(ct);
    }
}