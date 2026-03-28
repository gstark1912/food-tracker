namespace App.Api.Models;

public class WeeklySummary
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int WeekNumber { get; set; }
    public int WeeklyScore { get; set; }
    public decimal? WeightKg { get; set; }
    public DateTime CalculatedAt { get; set; }
}
