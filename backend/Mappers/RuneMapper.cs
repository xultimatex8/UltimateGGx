using backend.Models;
using backend.Models.Dtos;

namespace backend.Mappers;

public static class RuneMapper
{
    public static RuneDto RuneToRuneDto(Rune rune)
    {
        return new RuneDto
        {
            Name = rune.Name,
            Icon = rune.Icon
        };
    }
}
