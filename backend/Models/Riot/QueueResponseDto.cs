namespace backend.Models.Riot;

public class QueueResponseDto
{
    public string QueueType { get; set; } = default!;
    public string Tier { get; set; } = default!;
    public string Rank { get; set; } = default!;
    public int LeaguePoints { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
}