using backend.Models;
using backend.Models.Dtos;

namespace backend.Mappers;

public static class ChampionMapper
{
    public static ChampionDto ChampionToChampionDto(Champion champion)
    {
        return new ChampionDto
        {
            Name = champion.Name,
            RiotId = champion.RiotId
        };
    }
}