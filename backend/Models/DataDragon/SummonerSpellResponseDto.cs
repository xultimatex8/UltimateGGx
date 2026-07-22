namespace backend.Models.DataDragon;

public class SummonerSpellResponseDto
{
    public Dictionary<string, SummonerSpellDto> Data { get; set; } = [];
}

public class SummonerSpellDto
{
    public string Key { get; set; } = default!;
    public string Name { get; set; } = default!;
}