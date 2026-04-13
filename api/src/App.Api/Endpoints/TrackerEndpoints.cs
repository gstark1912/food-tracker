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
        app.MapGet("/api/tracker/days", GetDailyEntries);
        app.MapGet("/api/tracker/days/current-week", GetCurrentWeekEntries);
    }

    private static readonly string[] MomentOrder = ["Mañana", "Mediodía", "Tarde", "Noche"];

    private static List<MomentEntryDto> OrderedMoments(IEnumerable<MomentEntry> moments) =>
        moments.OrderBy(m => Array.IndexOf(MomentOrder, m.Moment))
               .Select(m => new MomentEntryDto(m.Moment, m.Food, m.Exercise))
               .ToList();

    internal static (int page, int pageSize) NormalizePaginationParams(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;
        return (page, pageSize);
    }

    private static async Task<IResult> GetDailyEntries(
        AppDbContext db, LocalClock clock,
        int page = 1, int pageSize = 10)
    {
        (page, pageSize) = NormalizePaginationParams(page, pageSize);

        var today = clock.Today;

        var totalCount = await db.DayEntries
            .CountAsync(d => d.IsFinalized && d.Date < today);

        var entries = await db.DayEntries
            .Include(d => d.Moments)
            .Where(d => d.IsFinalized && d.Date < today)
            .OrderByDescending(d => d.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = entries.Select(MapToDailyEntryItem).ToList();
        var hasMore = (page * pageSize) < totalCount;

        return Results.Ok(new DailyEntriesResponse(items, totalCount, hasMore));
    }
    private static async Task<IResult> GetCurrentWeekEntries(AppDbContext db, LocalClock clock)
    {
        var today = clock.Today;
        var dayOfWeek = today.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)today.DayOfWeek - 1;
        var weekStart = today.AddDays(-dayOfWeek);

        var entries = await db.DayEntries
            .Include(d => d.Moments)
            .Where(d => d.IsFinalized && d.Date >= weekStart && d.Date < today)
            .OrderByDescending(d => d.Date)
            .ToListAsync();

        var items = entries.Select(MapToDailyEntryItem).ToList();
        return Results.Ok(items);
    }

    internal static DailyEntryItem MapToDailyEntryItem(DayEntry entry)
    {
        var momentDict = entry.Moments.ToDictionary(m => m.Moment);

        int GetFood(string moment) =>
            momentDict.TryGetValue(moment, out var m) ? m.Food : 0;

        var totalExercise = entry.Moments.Sum(m => m.Exercise);

        return new DailyEntryItem(
            Date: entry.Date.ToString("yyyy-MM-dd"),
            FoodMañana: GetFood("Mañana"),
            FoodMediodia: GetFood("Mediodía"),
            FoodTarde: GetFood("Tarde"),
            FoodNoche: GetFood("Noche"),
            TotalExercise: totalExercise
        );
    }

    private static async Task<IResult> GetNextPending(AppDbContext db, LocalClock clock)
    {
        var today = clock.Today;

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

        // Create weekly summary if it doesn't exist yet
        var weekYear = ISOWeek.GetYear(parsedDate.ToDateTime(TimeOnly.MinValue));
        var weekNumber = ISOWeek.GetWeekOfYear(parsedDate.ToDateTime(TimeOnly.MinValue));
        await CalculateAndStoreWeeklySummary(db, weekYear, weekNumber);

        // Fix any past weeks that got stuck with score 0
        await RecalculateZeroScoreWeeks(db);

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
        var weekStart = DateOnly.FromDateTime(ISOWeek.ToDateTime(year, weekNumber, DayOfWeek.Monday));
        var weekEnd = weekStart.AddDays(6);

        var entries = await db.DayEntries
            .Include(d => d.Moments)
            .Where(d => d.IsFinalized && d.Date >= weekStart && d.Date <= weekEnd)
            .ToListAsync();

        var totalFood = entries.SelectMany(e => e.Moments).Sum(m => m.Food);
        var totalExercise = entries.SelectMany(e => e.Moments).Sum(m => m.Exercise);
        var score = totalFood - totalExercise;

        var existing = await db.WeeklySummaries
            .FirstOrDefaultAsync(w => w.Year == year && w.WeekNumber == weekNumber);

        if (existing != null)
        {
            existing.WeeklyScore = score;
            existing.CalculatedAt = DateTime.UtcNow;
        }
        else
        {
            db.WeeklySummaries.Add(new WeeklySummary
            {
                Id = Guid.NewGuid(),
                Year = year,
                WeekNumber = weekNumber,
                WeeklyScore = score,
                CalculatedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task RecalculateZeroScoreWeeks(AppDbContext db)
    {
        var zeroWeeks = await db.WeeklySummaries
            .Where(w => w.WeeklyScore == 0)
            .ToListAsync();

        foreach (var week in zeroWeeks)
        {
            var weekStart = DateOnly.FromDateTime(ISOWeek.ToDateTime(week.Year, week.WeekNumber, DayOfWeek.Monday));
            var weekEnd = weekStart.AddDays(6);

            var entries = await db.DayEntries
                .Include(d => d.Moments)
                .Where(d => d.IsFinalized && d.Date >= weekStart && d.Date <= weekEnd)
                .ToListAsync();

            var totalFood = entries.SelectMany(e => e.Moments).Sum(m => m.Food);
            var totalExercise = entries.SelectMany(e => e.Moments).Sum(m => m.Exercise);
            var score = totalFood - totalExercise;

            if (score != 0)
            {
                week.WeeklyScore = score;
                week.CalculatedAt = DateTime.UtcNow;
            }
        }

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
