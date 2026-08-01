namespace backend.Models;

public class SummonerSpell : BaseEntity
{
    public int Key { get; set; }
    public string Name { get; set; } = default!;
    public string RiotId { get; set; } = default!;
    
    public ICollection<Participant> Participants { get; set; } = [];
}