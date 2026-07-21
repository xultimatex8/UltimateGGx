namespace backend.Models;

public class ParticipantFrame : BaseEntity
{
    public int Gold { get; set; }
    public int Level { get; set; }
    public long Timestamp { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }

    public int ParticipantId { get; set; }
    public Participant Participant { get; set; } = default!;
}