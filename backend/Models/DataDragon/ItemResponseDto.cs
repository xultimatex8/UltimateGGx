namespace backend.Models.DataDragon;

public class ItemResponseDto
{
    public Dictionary<string, ItemDto> Data { get; set; } = [];
}

public class ItemDto
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public ItemGoldDto Gold { get; set; } = default!;
    public Dictionary<string, double> Stats { get; set; } = [];
}

public class ItemGoldDto
{
    public int Total { get; set; }
    public int Sell { get; set; }
}