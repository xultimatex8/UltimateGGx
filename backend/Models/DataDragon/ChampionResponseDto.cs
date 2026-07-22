namespace backend.Models.DataDragon;

public class ChampionResponseDto
{
    public Dictionary<string, ChampionDto> Data { get; set; } = [];
}

public class ChampionDto
{
    public string Key { get; set; } = default!;
    public string Name { get; set; } = default!;
    public List<string> Tags { get; set; } = [];
}