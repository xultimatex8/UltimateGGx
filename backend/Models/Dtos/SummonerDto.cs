namespace backend.Models.Dtos;

public class SummonerDto
{
    public string Puuid { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Tag { get; set; } = default!;
    public int Level { get; set; }
    public int ProfileIconId { get; set; }
    public DateTime LastUpdate { get; set; }
    public List<QueueDto> Queues { get; set; } = [];
}