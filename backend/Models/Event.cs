using backend.Models.Enums;

namespace backend.Models;

public class Event : BaseEntity
{
    public long Timestamp { get; set; }
    public int Bounty { get; set; }
    public int ShutdownBounty { get; set; }
    public string? MonsterType { get; set; }
    public string? MonsterSubType { get; set; }
    public string? BuildingType { get; set; }
    public string? LaneType { get; set; }
    public string? TowerType { get; set; }
    public EventType Type { get; set; }

    public string MatchId { get; set; } = default!;
    public Match Match { get; set; } = default!;

    public int? KillerParticipantId { get; set; }
    public Participant? Killer { get; set; }

    public int? VictimParticipantId { get; set; }
    public Participant? Victim { get; set; }

    public ICollection<Participant> AssistingParticipants { get; set; } = [];
}