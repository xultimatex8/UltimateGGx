using System.Text.RegularExpressions;
using backend.Data;
using backend.Interfaces;
using backend.Models;
using backend.Models.DataDragon;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ChampionSyncService
{
    private readonly IDataDragonService _dataDragonService;
    private readonly AppDbContext _db;

    public ChampionSyncService(IDataDragonService dataDragonService, AppDbContext db)
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
                    RiotId = championDto.Id,
                    Roles = championDto.Tags
                });
            }
            else
            {
                existing.Name = championDto.Name;
                existing.RiotId = championDto.Id;
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
                    Name = spellDto.Name,
                    RiotId = spellDto.Id
                });
            }
            else
            {
                existing.Name = spellDto.Name;
                existing.RiotId = spellDto.Id;
            }
        }

        ItemResponseDto itemsResponse = await _dataDragonService.GetItemsAsync(version, ct);
        foreach (var (itemId, itemDto) in itemsResponse.Data)
        {
            int key = int.Parse(itemId);
            Item? existing = await _db.Items
                .FirstOrDefaultAsync(i => i.Key == key, ct);

            if (existing is null)
            {
                _db.Items.Add(new Item
                {
                    Key = key,
                    Name = itemDto.Name,
                    Description = CleanDescription(itemDto.Description),
                    BuyPrice = itemDto.Gold.Total,
                    SellPrice = itemDto.Gold.Sell,
                    Stats = itemDto.Stats,
                });
            }
            else
            {
                existing.Name = itemDto.Name;
                existing.Description = CleanDescription(itemDto.Description);
                existing.BuyPrice = itemDto.Gold.Total;
                existing.SellPrice = itemDto.Gold.Sell;
                existing.Stats = itemDto.Stats;
            }
        }

        List<RuneResponseDto> runesResponse = await _dataDragonService.GetRunesAsync(version, ct);

        foreach (var styleDto in runesResponse)
        {
            Rune? styleExisting = await _db.Runes
                .FirstOrDefaultAsync(r => r.RiotId == styleDto.Id, ct);

            if (styleExisting is null)
            {
                _db.Runes.Add(new Rune
                {
                    Key = styleDto.Key,
                    Name = styleDto.Name,
                    RiotId = styleDto.Id,
                    Icon = styleDto.Icon,
                    IsStyle = true
                });
            }
            else
            {
                styleExisting.Name = styleDto.Name;
                styleExisting.Icon = styleDto.Icon;
            }

            foreach (var slot in styleDto.Slots)
            {
                foreach (var runeDto in slot.Runes)
                {
                    Rune? existing = await _db.Runes
                        .FirstOrDefaultAsync(r => r.RiotId == runeDto.Id, ct);

                    if (existing is null)
                    {
                        _db.Runes.Add(new Rune
                        {
                            RiotId = runeDto.Id,
                            Key = runeDto.Key,
                            Name = runeDto.Name,
                            Icon = runeDto.Icon,
                            IsStyle = false
                        });
                    }
                    else
                    {
                        existing.Name = runeDto.Name;
                        existing.Icon = runeDto.Icon;
                    }
                }
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    private static string CleanDescription(string description)
    {
        description = Regex.Replace(
            description,
            @"<stats>.*?</stats>",
            "",
            RegexOptions.Singleline);

        description = description.Replace("<mainText>", "")
                                .Replace("</mainText>", "");

        description = Regex.Replace(description, @"<br\s*/?>", "\n");

        description = description.Replace("<li>", "\n• ");

        description = Regex.Replace(description, "<.*?>", "");

        description = Regex.Replace(description, @"\n{2,}", "\n\n");

        return description.Trim();
    }
}