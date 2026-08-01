using backend.Models.Enums;

namespace backend.Models.Dtos;

public class MatchDto
{
    public string EndOfGameResult { get; set; } = default!;
    public long GameDuration { get; set; }
    public long GameEndTimestamp { get; set; }
    public QueueType QueueType { get; set; }
    public bool Win { get; set; }
    public List<ParticipantDetailDto> Participants { get; set; } = [];
}

public class ParticipantDetailDto
{
    public string SummonerName { get; set; } = default!;
    public string SummonerTag { get; set; } = default!;
    public int Assists { get; set; }
    public int ChampionLevel { get; set; }
    public int Deaths { get; set; }
    public int Gold { get; set; }
    public int Kills { get; set; }
    public string Lane { get; set; } = default!;
    public int Minions { get; set; }
    public int PrimaryRune { get; set; }
    public int SecondaryTree { get; set; }
    public int DamageToChampions { get; set; }
    public int TeamId { get; set; }
    public ChampionDto Champion { get; set; } = default!;
    public List<ItemDto> Items { get; set; } = [];
    public List<SummonerSpellDto> SummonerSpells { get; set; } = [];
}