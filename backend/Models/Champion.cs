namespace backend.Models;

public class Champion : BaseEntity
{
    public int Key { get; set; }
    public string Name { get; set; } = default!;
    public string RiotId { get; set; } = default!;
    public List<string> Roles { get; set; } = [];

    public ICollection<Participant> Participants { get; set; } = [];
}