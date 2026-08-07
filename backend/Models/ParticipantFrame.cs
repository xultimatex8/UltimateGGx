namespace backend.Models;

public class ParticipantFrame : BaseEntity
{
    public int CurrentGold { get; set; }
    public int Minions { get; set; }
    public int Level { get; set; }
    public long Timestamp { get; set; }
    public int TotalGold { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }

    public int ParticipantId { get; set; }
    public Participant Participant { get; set; } = default!;
}