namespace backend.Models.Riot;

public class TimelineResponseDto
{
    public InfoTimelineDto Info { get; set; } = default!;
}

public class InfoTimelineDto
{
    public List<FramesTimeLineDto> Frames { get; set; } = [];
}

public class FramesTimeLineDto
{
    public List<EventsTimeLineDto> Events { get; set; } = [];
    public Dictionary<string, ParticipantFrameDto> ParticipantFrames { get; set; } = [];
    public long Timestamp { get; set; }

}

public class EventsTimeLineDto
{
    public long Timestamp { get; set; }
    public int Bounty { get; set; }
    public int ShutdownBounty { get; set; }
    public string MonsterType { get; set; } = default!;
    public string MonsterSubType { get; set; } = default!;
    public string BuildingType { get; set; } = default!;
    public string LaneType { get; set; } = default!;
    public string TowerType { get; set; } = default!;
    public string Type { get; set; } = default!;
    public int KillerId { get; set; }
    public int ItemId { get; set; }
    public int BeforeId { get; set; }
    public int AfterId { get; set; }
    public int ParticipantId { get; set; }
    public int VictimId { get; set; }
    public int TeamId { get; set; }
    public int WinningTeam { get; set; }
    public List<int> AssistingParticipantIds { get; set; } = [];
}

public class ParticipantFrameDto
{
    public int CurrentGold { get; set; }
    public int JungleMinionsKilled { get; set; }
    public int MinionsKilled { get; set; }
    public int Level { get; set; }
    public PositionDto Position { get; set; } = default!;
    public int TotalGold { get; set; }
}

public class PositionDto
{
    public int X { get; set; }
    public int Y { get; set; }
}