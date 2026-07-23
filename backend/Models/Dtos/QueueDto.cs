using backend.Models.Enums;

namespace backend.Models.Dtos;

public class QueueDto
{
    public QueueType Type { get; set; } = default!;
    public string Tier { get; set; } = default!;
    public string Rank { get; set; } = default!;
    public int Points { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
}