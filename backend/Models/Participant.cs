namespace backend.Models;

public class Participant : BaseEntity
{
    public int ParticipantId { get; set; }

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
    public Team Team { get; set; } = default!;

    public int SummonerId { get; set; }
    public Summoner Summoner { get; set; } = default!;

    public int ChampionId { get; set; }
    public Champion Champion { get; set; } = default!;

    public ICollection<Item> Items { get; set; } = [];
    public ICollection<SummonerSpell> SummonerSpells { get; set; } = [];
    public ICollection<ParticipantFrame> Frames { get; set; } = [];
    public ICollection<Event> ParticipantInEvent { get; set; } = [];
    public ICollection<Event> KillsAsKiller { get; set; } = [];
    public ICollection<Event> DeathsAsVictim { get; set; } = [];
    public ICollection<Event> Assisted { get; set; } = [];
}