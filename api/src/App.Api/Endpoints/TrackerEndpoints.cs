using Microsoft.EntityFrameworkCore;
using System.Globalization;
using App.Api.Data;
using App.Api.Models;

namespace App.Api.Endpoints;

public static class TrackerEndpoints
{
    public static void MapTrackerEndpoints(this WebApplication app)
    {
        app.MapGet("/api/tracker/next-pending", GetNextPending);
        app.MapGet("/api/tracker/day/{date}", GetDayByDate);
        app.MapPut("/api/tracker/day/{date}", SaveDay);
        app.MapPost("/api/tracker/day/{date}/finalize", FinalizeDay);
    }

    private static readonly string[] MomentOrder = ["Mañana", "Mediodía", "Tarde", "Noche"];

    private static List<MomentEntryDto> OrderedMoments(IEnumerable<MomentEntry> moments) =>
        moments.OrderBy(m => Array.IndexOf(MomentOrder, m.Moment))
               .Select(m => new MomentEntryDto(m.Moment, m.Food, m.Exercise))
               .ToList();

    private static async Task<IResult> GetNextPending(AppDbContext db, LocalClock clock)
    {
        var today = clock.Today;

        var currentWeek = ISOWeek.GetWeekOfYear(today.ToDateTime(TimeOnly.MinValue));
        var currentYear = ISOWeek.GetYear(today.ToDateTime(TimeOnly.MinValue));

        var existingSummary = await db.WeeklySummaries
            .FirstOrDefaultAsync(w => w.Year == currentYear && w.WeekNumber == currentWeek);

        if (existingSummary == null)
            await CalculateAndStoreWeeklySummary(db, currentYear, currentWeek);

        var oldestPending = await db.DayEntries
            .Where(d => d.Date < today && !d.IsFinalized)
            .OrderBy(d => d.Date)
            .FirstOrDefaultAsync();

        var targetDate = oldestPending?.Date ?? today;
        var entry = await GetOrCreateDayEntry(db, targetDate);

        return Results.Ok(new NextPendingResponse(
            entry.Date.ToString("yyyy-MM-dd"),
            entry.Date == today,
            entry.IsFinalized,
            OrderedMoments(entry.Moments)
        ));
    }

    private static async Task<IResult> GetDayByDate(string date, AppDbContext db, LocalClock clock)
    {
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out var parsedDate))
            return Results.BadRequest(new { error = $"Formato de fecha inválido: {date}. Use yyyy-MM-dd" });

        var entry = await db.DayEntries
            .Include(d => d.Moments)
            .FirstOrDefaultAsync(d => d.Date == parsedDate);

        if (entry == null)
            return Results.NotFound(new { error = $"No existe registro para la fecha {date}" });

        return Results.Ok(new NextPendingResponse(
            entry.Date.ToString("yyyy-MM-dd"),
            entry.Date == clock.Today,
            entry.IsFinalized,
            OrderedMoments(entry.Moments)
        ));
    }

    private static async Task<IResult> SaveDay(string date, SaveDayRequest request, AppDbContext db, LocalClock clock)
    {
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out var parsedDate))
            return Results.BadRequest(new { error = $"Formato de fecha inválido: {date}. Use yyyy-MM-dd" });

        var today = clock.Today;
        if (parsedDate > today)
            return Results.BadRequest(new { error = $"No se puede editar un día futuro. Fecha recibida: {date}" });

        if (request.Moments == null || request.Moments.Count != 4)
            return Results.BadRequest(new { error = "Se requieren exactamente 4 momentos" });

        foreach (var m in request.Moments)
        {
            if (!Fibonacci.IsValid(m.Food))
                return Results.BadRequest(new { error = $"El valor {m.Food} no es válido para Comida en {m.Moment}. Valores permitidos: 0, 1, 2, 3, 5, 8, 13" });
            if (!Fibonacci.IsValid(m.Exercise))
                return Results.BadRequest(new { error = $"El valor {m.Exercise} no es válido para Ejercicio en {m.Moment}. Valores permitidos: 0, 1, 2, 3, 5, 8, 13" });
        }

        var entry = await db.DayEntries
            .Include(d => d.Moments)
            .FirstOrDefaultAsync(d => d.Date == parsedDate);

        if (entry != null && entry.IsFinalized)
            return Results.BadRequest(new { error = $"El día {date} ya fue finalizado y no puede modificarse" });

        if (entry == null)
        {
            entry = new DayEntry
            {
                Id = Guid.NewGuid(),
                Date = parsedDate,
                IsFinalized = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Moments = request.Moments.Select(m => new MomentEntry
                {
                    Id = Guid.NewGuid(),
                    Moment = m.Moment,
                    Food = m.Food,
                    Exercise = m.Exercise
                }).ToList()
            };
            db.DayEntries.Add(entry);
        }
        else
        {
            entry.UpdatedAt = DateTime.UtcNow;
            foreach (var req in request.Moments)
            {
                var existing = entry.Moments.FirstOrDefault(m => m.Moment == req.Moment);
                if (existing != null)
                {
                    existing.Food = req.Food;
                    existing.Exercise = req.Exercise;
                }
                else
                {
                    entry.Moments.Add(new MomentEntry
                    {
                        Id = Guid.NewGuid(),
                        DayEntryId = entry.Id,
                        Moment = req.Moment,
                        Food = req.Food,
                        Exercise = req.Exercise
                    });
                }
            }
        }

        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> FinalizeDay(string date, AppDbContext db, LocalClock clock)
    {
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out var parsedDate))
            return Results.BadRequest(new { error = $"Formato de fecha inválido: {date}. Use yyyy-MM-dd" });

        var entry = await db.DayEntries
            .Include(d => d.Moments)
            .FirstOrDefaultAsync(d => d.Date == parsedDate);

        if (entry == null)
            return Results.NotFound(new { error = $"No existe registro para la fecha {date}" });

        if (entry.IsFinalized)
            return Results.BadRequest(new { error = $"El día {date} ya fue finalizado" });

        entry.IsFinalized = true;
        entry.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var today = clock.Today;
        var nextPending = await db.DayEntries
            .Where(d => d.Date < today && !d.IsFinalized)
            .OrderBy(d => d.Date)
            .FirstOrDefaultAsync();

        var nextDate = nextPending?.Date ?? today;
        return Results.Ok(new FinalizeDayResponse(nextDate.ToString("yyyy-MM-dd")));
    }

    private static async Task CalculateAndStoreWeeklySummary(AppDbContext db, int year, int weekNumber)
    {
        var existing = await db.WeeklySummaries
            .FirstOrDefaultAsync(w => w.Year == year && w.WeekNumber == weekNumber);
        if (existing != null) return;

        var weekStart = DateOnly.FromDateTime(ISOWeek.ToDateTime(year, weekNumber, DayOfWeek.Monday));
        var weekEnd = weekStart.AddDays(6);

        var entries = await db.DayEntries
            .Include(d => d.Moments)
            .Where(d => d.Date >= weekStart && d.Date <= weekEnd)
            .ToListAsync();

        var totalFood = entries.SelectMany(e => e.Moments).Sum(m => m.Food);
        var totalExercise = entries.SelectMany(e => e.Moments).Sum(m => m.Exercise);

        db.WeeklySummaries.Add(new WeeklySummary
        {
            Id = Guid.NewGuid(),
            Year = year,
            WeekNumber = weekNumber,
            WeeklyScore = totalFood - totalExercise,
            CalculatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }

    private static async Task<DayEntry> GetOrCreateDayEntry(AppDbContext db, DateOnly date)
    {
        var entry = await db.DayEntries
            .Include(d => d.Moments)
            .FirstOrDefaultAsync(d => d.Date == date);

        if (entry != null) return entry;

        var moments = new[] { "Mañana", "Mediodía", "Tarde", "Noche" };
        var newEntry = new DayEntry
        {
            Id = Guid.NewGuid(),
            Date = date,
            IsFinalized = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Moments = moments.Select(m => new MomentEntry
            {
                Id = Guid.NewGuid(),
                Moment = m,
                Food = 0,
                Exercise = 0
            }).ToList()
        };

        db.DayEntries.Add(newEntry);
        await db.SaveChangesAsync();
        return newEntry;
    }
}
