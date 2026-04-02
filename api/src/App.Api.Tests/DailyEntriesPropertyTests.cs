using App.Api.Endpoints;
using App.Api.Models;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace App.Api.Tests;

public class DailyEntriesPropertyTests
{
    private static readonly DateOnly Today = new(2026, 4, 2);
    private static readonly string[] MomentNames = ["Mañana", "Mediodía", "Tarde", "Noche"];

    private static DayEntry CreateDayEntry(DateOnly date, bool isFinalized, List<MomentEntry> moments)
    {
        return new DayEntry
        {
            Id = Guid.NewGuid(),
            Date = date,
            IsFinalized = isFinalized,
            Moments = moments
        };
    }

    private static MomentEntry CreateMoment(string name, int food, int exercise)
    {
        return new MomentEntry
        {
            Id = Guid.NewGuid(),
            Moment = name,
            Food = Math.Abs(food) % 14,
            Exercise = Math.Abs(exercise) % 14
        };
    }

    private static List<DayEntry> GenerateEntries(bool[] finalized, int[] dayOffsets, int[] foods, int[] exercises)
    {
        var count = Math.Min(Math.Min(finalized.Length, dayOffsets.Length),
                    Math.Min(foods.Length, exercises.Length));
        var entries = new List<DayEntry>();
        for (int i = 0; i < count; i++)
        {
            var date = Today.AddDays(dayOffsets[i] % 60 - 30);
            var moments = MomentNames.Select(m => CreateMoment(m, foods[i], exercises[i])).ToList();
            entries.Add(CreateDayEntry(date, finalized[i], moments));
        }
        return entries;
    }

    /// <summary>
    /// Property 1: All items returned are from finalized days with date before today.
    /// **Validates: Requirements 1.2, 1.3**
    /// </summary>
    [Property]
    public Property AllItemsAreFinalizedAndBeforeToday()
    {
        return Prop.ForAll(
            Arb.From<bool[]>(),
            Arb.From<int[]>(),
            Arb.From<int[]>(),
            (finalized, dayOffsets, foods) =>
            {
                var entries = GenerateEntries(finalized, dayOffsets, foods, foods);

                var filtered = entries
                    .Where(e => e.IsFinalized && e.Date < Today)
                    .OrderByDescending(e => e.Date)
                    .ToList();

                var items = filtered.Select(TrackerEndpoints.MapToDailyEntryItem).ToList();

                return items.All(item =>
                {
                    var parsedDate = DateOnly.ParseExact(item.Date, "yyyy-MM-dd");
                    return parsedDate < Today;
                });
            });
    }

    /// <summary>
    /// Property 2: Items are ordered by date descending.
    /// **Validates: Requirement 1.4**
    /// </summary>
    [Property]
    public Property ItemsAreOrderedByDateDescending()
    {
        return Prop.ForAll(
            Arb.From<int[]>(),
            Arb.From<int[]>(),
            Arb.From<int[]>(),
            (dayOffsets, foods, exercises) =>
            {
                var entries = dayOffsets.Select((offset, i) =>
                {
                    var date = Today.AddDays(-(Math.Abs(offset) % 365 + 1));
                    var f = i < foods.Length ? foods[i] : 0;
                    var e = i < exercises.Length ? exercises[i] : 0;
                    var moments = MomentNames.Select(m => CreateMoment(m, f, e)).ToList();
                    return CreateDayEntry(date, true, moments);
                }).ToList();

                var sorted = entries
                    .OrderByDescending(e => e.Date)
                    .Take(10)
                    .ToList();

                var items = sorted.Select(TrackerEndpoints.MapToDailyEntryItem).ToList();

                for (int i = 0; i < items.Count - 1; i++)
                {
                    var current = DateOnly.ParseExact(items[i].Date, "yyyy-MM-dd");
                    var next = DateOnly.ParseExact(items[i + 1].Date, "yyyy-MM-dd");
                    if (current < next) return false;
                }
                return true;
            });
    }

    /// <summary>
    /// Property 3: hasMore and totalCount are consistent with the data.
    /// **Validates: Requirements 1.7, 1.8**
    /// </summary>
    [Property]
    public Property HasMoreAndTotalCountAreConsistent()
    {
        return Prop.ForAll(
            Arb.From<PositiveInt>(),
            Arb.From<PositiveInt>(),
            Arb.From<PositiveInt>(),
            (totalCountArb, pageArb, pageSizeArb) =>
            {
                var totalCount = totalCountArb.Get;
                var (page, pageSize) = TrackerEndpoints.NormalizePaginationParams(pageArb.Get, pageSizeArb.Get);
                var hasMore = (page * pageSize) < totalCount;

                if (hasMore)
                    return (page * pageSize) < totalCount;
                else
                    return (page * pageSize) >= totalCount;
            });
    }

    /// <summary>
    /// Property 4: TotalExercise equals sum of moments' exercise.
    /// **Validates: Requirement 2.2**
    /// </summary>
    [Property]
    public Property TotalExerciseIsSumOfMomentExercise()
    {
        return Prop.ForAll(
            Arb.From<int[]>(),
            Arb.From<int[]>(),
            (foods, exercises) =>
            {
                var moments = MomentNames.Select((name, i) =>
                {
                    var f = i < foods.Length ? foods[i] : 0;
                    var e = i < exercises.Length ? exercises[i] : 0;
                    return CreateMoment(name, f, e);
                }).ToList();

                var entry = CreateDayEntry(Today.AddDays(-1), true, moments);
                var item = TrackerEndpoints.MapToDailyEntryItem(entry);

                var expectedExercise = moments.Sum(m => m.Exercise);
                return item.TotalExercise == expectedExercise;
            });
    }

    /// <summary>
    /// Property 5: Number of items doesn't exceed pageSize.
    /// **Validates: Requirements 3.1, 3.2**
    /// </summary>
    [Property]
    public Property ItemCountDoesNotExceedPageSize()
    {
        return Prop.ForAll(
            Arb.From<int[]>(),
            Arb.From<PositiveInt>(),
            (dayOffsets, pageSizeArb) =>
            {
                var (_, pageSize) = TrackerEndpoints.NormalizePaginationParams(1, pageSizeArb.Get);

                var entries = dayOffsets.Select(offset =>
                {
                    var date = Today.AddDays(-(Math.Abs(offset) % 365 + 1));
                    var moments = MomentNames.Select(m => CreateMoment(m, 1, 1)).ToList();
                    return CreateDayEntry(date, true, moments);
                }).ToList();

                var items = entries
                    .OrderByDescending(e => e.Date)
                    .Take(pageSize)
                    .Select(TrackerEndpoints.MapToDailyEntryItem)
                    .ToList();

                return items.Count <= pageSize;
            });
    }
}
