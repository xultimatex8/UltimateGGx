using backend.Models.Enums;

namespace backend.Models.Dtos;

public class TimelineDto
{
    public List<EventDto> Events { get; set; } = [];
}

public class EventDto
{
    public long Timestamp { get; set; }

    public int? Bounty { get; set; }
    public int? ShutdownBounty { get; set; }

    public MonsterType? MonsterType { get; set; }
    public MonsterSubType? MonsterSubType { get; set; }

    public BuildingType? BuildingType { get; set; }
    public LaneType? LaneType { get; set; }
    public TowerType? TowerType { get; set; }

    public int? TeamId { get; set; }

    public EventType Type { get; set; }

    public int? MainParticipantId { get; set; }
    public int? VictimParticipantId { get; set; }
    public List<int> AssistingParticipants { get; set; } = [];

    public ItemDto? Item { get; set; }
    public ItemDto? BeforeItem { get; set; }
    public ItemDto? AfterItem { get; set; }
}