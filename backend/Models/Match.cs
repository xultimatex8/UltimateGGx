using backend.Models.Enums;

namespace backend.Models;

public class Match : BaseEntity
{
    public string EndOfGameResult { get; set; } = default!;
    public long GameDuration { get; set; }
    public long GameEndTimestamp { get; set; }
    public QueueType QueueType { get; set; }

    public MatchReference MatchReference { get; set; } = default!;
    public ICollection<Team> Teams { get; set; } = [];
    public ICollection<Event> Events { get; set; } = [];
}