namespace App.Api.Models;

public class DayEntry
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public bool IsFinalized { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<MomentEntry> Moments { get; set; } = [];
}
