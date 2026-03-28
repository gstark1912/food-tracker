namespace App.Api.Models;

public class MomentEntry
{
    public Guid Id { get; set; }
    public Guid DayEntryId { get; set; }
    public DayEntry DayEntry { get; set; } = null!;
    public string Moment { get; set; } = "";
    public int Food { get; set; }
    public int Exercise { get; set; }
}
