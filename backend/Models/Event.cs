using backend.Models.Enums;

namespace backend.Models;

public class Event : BaseEntity
{
    public long Timestamp { get; set; }

    public int? Bounty { get; set; }
    public int? ShutdownBounty { get; set; }

    public MonsterType? MonsterType { get; set; }
    public MonsterSubType? MonsterSubType { get; set; }

    public BuildingType? BuildingType { get; set; }
    public LaneType? LaneType { get; set; }
    public TowerType? TowerType { get; set; }

    public EventType Type { get; set; }

    public int MatchId { get; set; }
    public Match Match { get; set; } = default!;

    public int? ItemId { get; set; }
    public Item? Item { get; set; }

    public int? BeforeItemId { get; set; }
    public Item? BeforeItem { get; set; }

    public int? AfterItemId { get; set; }
    public Item? AfterItem { get; set; }

    public int? ParticipantId { get; set; }
    public Participant? Participant { get; set; }

    public int? KillerParticipantId { get; set; }
    public Participant? Killer { get; set; }

    public int? VictimParticipantId { get; set; }
    public Participant? Victim { get; set; }

    public ICollection<Participant> AssistingParticipants { get; set; } = [];
}