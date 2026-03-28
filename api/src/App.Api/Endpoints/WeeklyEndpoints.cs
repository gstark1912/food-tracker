using Microsoft.EntityFrameworkCore;
using System.Globalization;
using App.Api.Data;
using App.Api.Models;

namespace App.Api.Endpoints;

public static class WeeklyEndpoints
{
    public static void MapWeeklyEndpoints(this WebApplication app)
    {
        app.MapGet("/api/weekly", GetAllWeeklySummaries);
        app.MapGet("/api/weekly/{year}/{week}", GetWeeklySummary);
        app.MapPost("/api/weekly/{year}/{week}/weight", RegisterWeight);
    }

    private static async Task<IResult> GetAllWeeklySummaries(AppDbContext db)
    {
        var summaries = await db.WeeklySummaries
            .OrderByDescending(w => w.Year)
            .ThenByDescending(w => w.WeekNumber)
            .ToListAsync();

        return Results.Ok(summaries.Select(w => ToResponse(w)));
    }

    private static async Task<IResult> GetWeeklySummary(int year, int week, AppDbContext db)
    {
        var summary = await db.WeeklySummaries
            .FirstOrDefaultAsync(w => w.Year == year && w.WeekNumber == week);

        if (summary == null)
            return Results.NotFound(new { error = $"No existe resumen para la semana {year}-W{week}" });

        return Results.Ok(ToResponse(summary));
    }

    private static async Task<IResult> RegisterWeight(int year, int week, RegisterWeightRequest request, AppDbContext db)
    {
        var summary = await db.WeeklySummaries
            .FirstOrDefaultAsync(w => w.Year == year && w.WeekNumber == week);

        if (summary == null)
            return Results.NotFound(new { error = $"No existe resumen para la semana {year}-W{week}" });

        if (summary.WeightKg.HasValue)
            return Results.BadRequest(new { error = $"El peso ya fue registrado para la semana {year}-W{week}" });

        summary.WeightKg = request.WeightKg;
        await db.SaveChangesAsync();

        return Results.Ok(ToResponse(summary));
    }

    private static WeeklySummaryResponse ToResponse(WeeklySummary w)
    {
        var weekStart = DateOnly.FromDateTime(ISOWeek.ToDateTime(w.Year, w.WeekNumber, DayOfWeek.Monday));
        var weekEnd = weekStart.AddDays(6);
        return new WeeklySummaryResponse(
            w.Year,
            w.WeekNumber,
            w.WeeklyScore,
            w.WeightKg,
            weekStart.ToString("yyyy-MM-dd"),
            weekEnd.ToString("yyyy-MM-dd")
        );
    }
}
