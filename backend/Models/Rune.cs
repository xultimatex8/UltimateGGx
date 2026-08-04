namespace backend.Models;

public class Rune : BaseEntity
{
    public string Key { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int RiotId { get; set; }
    public string Icon { get; set; } = default!;
    public bool IsStyle { get; set; }
}