using backend.Models.Enums;

namespace backend.Models;

public class MatchReference : BaseEntity
{
    public string MatchId { get; set; } = default!;
    public QueueType QueueType { get; set; }

    public int? MatchDbId { get; set; }
    public Match? Match { get; set; }

    public ICollection<Summoner> Summoners { get; set; } = [];
}