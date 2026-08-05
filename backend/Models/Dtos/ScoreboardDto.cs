namespace backend.Models.Dtos;

public class ScoreboardDto
{
    public List<ScoreboardTeamDto> Teams { get; set; } = [];
    public long Timestamp { get; set; }
}

public class ScoreboardTeamDto
{
    public int TeamId { get; set; }
    public List<ScoreboardParticipantDto> Participants { get; set; } = [];
}

public class ScoreboardParticipantDto
{
    public int ParticipantId { get; set; }
    public string SummonerName { get; set; } = default!;
    public int Assists { get; set; }
    public int ChampionLevel { get; set; }
    public int Deaths { get; set; }
    public int CurrentGold { get; set; }
    public int TotalGold { get; set; }
    public int Kills { get; set; }
    public string Lane { get; set; } = default!;
    public int Minions { get; set; }
    public RuneDto PrimaryRune { get; set; } = default!;
    public RuneDto SecondaryTree { get; set; } = default!;
    public ChampionDto Champion { get; set; } = default!;
    public List<SummonerSpellDto> SummonerSpells { get; set; } = [];
    public List<ItemDto> Items { get; set; } = [];
}