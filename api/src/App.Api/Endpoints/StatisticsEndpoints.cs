using Microsoft.EntityFrameworkCore;
using System.Globalization;
using App.Api.Data;
using App.Api.Models;

namespace App.Api.Endpoints;

public static class StatisticsEndpoints
{
    public static void MapStatisticsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/statistics");
        group.MapGet("/", GetStatistics);
        group.MapGet("/kpis", async (AppDbContext db, LocalClock clock) =>
        {
            var today = clock.Today;
            var todayDt = today.ToDateTime(TimeOnly.MinValue);
            var currentYear = ISOWeek.GetYear(todayDt);
            var currentWeek = ISOWeek.GetWeekOfYear(todayDt);

            // Current week date range (Monday-Sunday)
            var currentWeekStart = DateOnly.FromDateTime(ISOWeek.ToDateTime(currentYear, currentWeek, DayOfWeek.Monday));
            var currentWeekEnd = currentWeekStart.AddDays(6);

            var currentWeekDays = await db.DayEntries
                .Include(d => d.Moments)
                .Where(d => d.IsFinalized && d.Date >= currentWeekStart && d.Date <= currentWeekEnd)
                .ToListAsync();

            var currentWeekKpi = CalculateWeeklyKpi(currentWeekDays, currentYear, currentWeek);

            // Previous week
            int prevYear, prevWeek;
            if (currentWeek == 1)
            {
                prevYear = currentYear - 1;
                prevWeek = ISOWeek.GetWeeksInYear(prevYear);
            }
            else
            {
                prevYear = currentYear;
                prevWeek = currentWeek - 1;
            }

            var prevWeekStart = DateOnly.FromDateTime(ISOWeek.ToDateTime(prevYear, prevWeek, DayOfWeek.Monday));
            var prevWeekEnd = prevWeekStart.AddDays(6);

            var prevWeekDays = await db.DayEntries
                .Include(d => d.Moments)
                .Where(d => d.IsFinalized && d.Date >= prevWeekStart && d.Date <= prevWeekEnd)
                .ToListAsync();

            WeeklyKpiData? previousWeekKpi = prevWeekDays.Count > 0
                ? CalculateWeeklyKpi(prevWeekDays, prevYear, prevWeek)
                : null;

            return Results.Ok(new WeeklyKpiResponse(currentWeekKpi, previousWeekKpi));
        });
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

    internal static WeeklyKpiData CalculateWeeklyKpi(List<DayEntry> finalizedDays, int year, int weekNumber)
    {
        if (finalizedDays.Count == 0)
        {
            return new WeeklyKpiData(year, weekNumber, 0m, 0, 0);
        }

        var totalFood = finalizedDays
            .SelectMany(d => d.Moments)
            .Sum(m => m.Food);

        var totalExercise = finalizedDays
            .SelectMany(d => d.Moments)
            .Sum(m => m.Exercise);

        var avgFood = Math.Round((decimal)totalFood / finalizedDays.Count, 1, MidpointRounding.AwayFromZero);

        return new WeeklyKpiData(year, weekNumber, avgFood, totalExercise, finalizedDays.Count);
    }
}
