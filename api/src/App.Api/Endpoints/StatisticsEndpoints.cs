using Microsoft.EntityFrameworkCore;
using System.Globalization;
using App.Api.Data;
using App.Api.Models;

namespace App.Api.Endpoints;

public static class StatisticsEndpoints
{
    public static void MapStatisticsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/statistics", GetStatistics);
    }

    private static async Task<IResult> GetStatistics(AppDbContext db, int n = 10)
    {
        var summaries = await db.WeeklySummaries
            .OrderBy(w => w.Year)
            .ThenBy(w => w.WeekNumber)
            .ToListAsync();

        // Take last N
        var recent = summaries.TakeLast(n).ToList();

        var result = new List<WeeklySummaryWithTrend>();
        for (int i = 0; i < recent.Count; i++)
        {
            var current = recent[i];
            string? trend = null;

            if (i > 0)
            {
                var previous = recent[i - 1];
                trend = current.WeeklyScore < previous.WeeklyScore ? "down" : "up";
            }

            var weekStart = DateOnly.FromDateTime(ISOWeek.ToDateTime(current.Year, current.WeekNumber, DayOfWeek.Monday));
            var weekEnd = weekStart.AddDays(6);

            result.Add(new WeeklySummaryWithTrend(
                new WeeklySummaryResponse(
                    current.Year,
                    current.WeekNumber,
                    current.WeeklyScore,
                    current.WeightKg,
                    weekStart.ToString("yyyy-MM-dd"),
                    weekEnd.ToString("yyyy-MM-dd")
                ),
                trend
            ));
        }

        // Return in descending order for display
        result.Reverse();
        return Results.Ok(new StatisticsResponse(result));
    }
}
