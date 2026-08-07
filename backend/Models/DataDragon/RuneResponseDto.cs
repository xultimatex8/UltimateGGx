namespace backend.Models.DataDragon;

public class RuneResponseDto
{
    public int Id { get; set; }
    public string Key { get; set; } = default!;
    public string Icon { get; set; } = default!;
    public string Name { get; set; } = default!;
    public List<RuneSlotDto> Slots { get; set; } = [];
}

public class RuneSlotDto
{
    public List<RuneDto> Runes { get; set; } = [];
}

public class RuneDto
{
    public int Id { get; set; }
    public string Key { get; set; } = default!;
    public string Icon { get; set; } = default!;
    public string Name { get; set; } = default!;
}