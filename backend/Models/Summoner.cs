namespace backend.Models;

public class Summoner : BaseEntity
{
    public string Puuid { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Tag { get; set; } = default!;
    public int Level { get; set; }
    public int ProfileIconId { get; set; }

    public ICollection<Queue> Queues { get; set; } = [];
}