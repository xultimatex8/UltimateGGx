namespace backend.Models;

public class DataDragonState : BaseEntity
{
    public new int Id { get; set; } = 1;
    public string CurrentVersion { get; set; } = default!;
    public DateTime LastCheckedAt { get; set; }
}