namespace backend.Models;

public class Item : BaseEntity
{
    public int Key { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public Dictionary<string, double> Stats { get; set; } = [];

    public ICollection<Participant> Participants { get; set; } = [];
    public ICollection<Event> Events { get; set; } = [];
    public ICollection<Event> BeforeEvents { get; set; } = [];
    public ICollection<Event> AfterEvents { get; set; } = [];
}