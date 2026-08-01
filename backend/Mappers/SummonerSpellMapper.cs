using backend.Models;
using backend.Models.Dtos;

namespace backend.Mappers;

public static class SummonerSpellMapper
{
    public static SummonerSpellDto SummonerSpellToSummonerSpellDto(SummonerSpell summonerSpell)
    {
        return new SummonerSpellDto
        {
            RiotId = summonerSpell.RiotId,
            Name = summonerSpell.Name
        };
    }
}