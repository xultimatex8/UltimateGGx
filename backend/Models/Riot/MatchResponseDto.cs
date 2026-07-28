namespace backend.Models.Riot;

public class MatchResponseDto
{
    public InfoDto Info { get; set; } = default!;
}

public class InfoDto
{
    public string EndOfGameResult { get; set; } = default!;
    public long GameDuration { get; set; }
    public long GameEndTimestamp { get; set; }
    public int QueueId { get; set; }
    public List<ParticipantDto> Participants { get; set; } = [];
    public List<TeamDto> Teams { get; set; } = [];
}

public class ParticipantDto
{
    public int Assists { get; set; }
    public int ChampLevel { get; set; }
    public int ChampionId { get; set; }
    public int Deaths { get; set; }
    public int GoldEarned { get; set; }
    public int Item0 { get; set; }
    public int Item1 { get; set; }
    public int Item2 { get; set; }
    public int Item3 { get; set; }
    public int Item4 { get; set; }
    public int Item5 { get; set; }
    public int Item6 { get; set; }
    public int Kills { get; set; }
    public string TeamPosition { get; set; } = default!;
    public int ParticipantId { get; set; }
    public PerksDto Perks { get; set; } = default!;
    public int ProfileIcon { get; set; }
    public string RiotIdTagLine { get; set; } = default!;
    public string Puuid { get; set; } = default!;
    public int Summoner1Id { get; set; }
    public int Summoner2Id { get; set; }
    public string RiotIdGameName { get; set; } = default!;
    public int SummonerLevel { get; set; }
    public int TeamId { get; set; }
    public int TotalDamageDealtToChampions { get; set; }
}

public class PerksDto
{
    public List<PerkStyleDto> Styles { get; set; } = [];
}

public class PerkStyleDto
{
    public List<PerkStyleSelectionDto> Selections { get; set; } = default!;
    public int Style { get; set; }
}

public class PerkStyleSelectionDto
{
    public int Perk { get; set; }
}

public class TeamDto
{
    public int TeamId { get; set; }
    public bool Win { get; set; }
}