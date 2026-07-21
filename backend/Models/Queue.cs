using backend.Models.Enums;

namespace backend.Models;

public class Queue : BaseEntity
{
    public QueueType Type { get; set; }
    public string Tier { get; set; } = default!;
    public string Rank { get; set; } = default!;
    public int Points { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }

    public string SummonerId { get; set; } = default!;
    public Summoner Summoner { get; set; } = default!;
}