namespace backend.Models.Dtos;

public class ItemDto
{
    public int Key { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public Dictionary<string, double> Stats { get; set; } = [];
}