namespace backend.Models;

public class Team : BaseEntity
{
    public bool Win { get; set; }
    public int TeamId { get; set; }

    public int MatchId { get; set; }
    public Match Match { get; set; } = default!;
    
    public ICollection<Participant> Participants { get; set; } = [];
}