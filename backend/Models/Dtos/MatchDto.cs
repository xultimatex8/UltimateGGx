using backend.Models.Enums;

namespace backend.Models.Dtos;

public class MatchDto
{
    public string ChampionName { get; set; } = default!;
    public string EndOfGameResult { get; set; } = default!;
    public long GameDuration { get; set; }
    public long GameEndTimestamp { get; set; }
    public QueueType QueueType { get; set; }
    public bool Win { get; set; }
    public int Assists { get; set; }
    public int ChampionLevel { get; set; }
    public int Deaths { get; set; }
    public int Gold { get; set; }
    public List<int> Items { get; set; } = [];
    public int Kills { get; set; }
    public string Lane { get; set; } = default!;
    public int Minions { get; set; }
    public int PrimaryRune { get; set; }
    public int SecondaryTree { get; set; }
    public int DamageToChampions { get; set; }
    public List<ParticipantBriefDto> Participants { get; set; } = [];
    public List<SummonerSpellDto> SummonerSpells { get; set; } = [];
}

public class ParticipantBriefDto
{
    public string ChampionName { get; set; } = default!;
    public string SummonerName { get; set; } = default!;
    public string Lane { get; set; } = default!;
    public int Minions { get; set; }
    public int TeamId { get; set; }
}

public class SummonerSpellDto
{
    public int Key { get; set; }
    public string Name { get; set; } = default!;
}