using App.Api.Endpoints;
using App.Api.Models;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace App.Api.Tests;

/// <summary>
/// Property-based tests for StatisticsEndpoints.CalculateWeeklyKpi
/// </summary>
public class KpiCalculationTests
{
    private static readonly int[] FibonacciValues = [0, 1, 2, 3, 5, 8, 13];

    private static Gen<int> GenFibonacci =>
        Gen.Elements(FibonacciValues);

    private static Gen<MomentEntry> GenMomentEntry(Guid dayEntryId) =>
        from food in GenFibonacci
        from exercise in GenFibonacci
        select new MomentEntry
        {
            Id = Guid.NewGuid(),
            DayEntryId = dayEntryId,
            Moment = "Desayuno",
            Food = food,
            Exercise = exercise
        };

    private static Gen<DayEntry> GenDayEntry =>
        from momentCount in Gen.Choose(1, 5)
        let dayId = Guid.NewGuid()
        from moments in GenMomentEntry(dayId).ListOf(momentCount)
        from dayOffset in Gen.Choose(0, 6)
        select new DayEntry
        {
            Id = dayId,
            Date = DateOnly.FromDateTime(DateTime.Today).AddDays(-dayOffset),
            IsFinalized = true,
            Moments = moments.ToList()
        };

    private static Arbitrary<List<DayEntry>> ArbDayEntries() =>
        GenDayEntry.ListOf().Select(l => l.ToList()).ToArbitrary();

    /// <summary>
    /// Feature: statistics-kpi-dashboard, Property 1: Cálculo del promedio de comida semanal
    /// Validates: Requirements 1.1, 1.5, 2.1, 2.2
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AvgFood_Equals_SumFood_DividedBy_MomentCount()
    {
        return Prop.ForAll(ArbDayEntries(), days =>
        {
            var result = StatisticsEndpoints.CalculateWeeklyKpi(days, 2025, 1);

            if (days.Count == 0)
            {
                return result.AvgFood == 0m;
            }

            var allMoments = days.SelectMany(d => d.Moments).ToList();
            if (allMoments.Count == 0)
            {
                return result.AvgFood == 0m;
            }

            var totalFood = allMoments.Sum(m => m.Food);
            var expected = Math.Round((decimal)totalFood / allMoments.Count, 1, MidpointRounding.AwayFromZero);
            return result.AvgFood == expected;
        });
    }

    /// <summary>
    /// Feature: statistics-kpi-dashboard, Property 2: Cálculo del total de ejercicio semanal
    /// Validates: Requirements 1.2, 3.1, 3.2
    /// </summary>
    [Property(MaxTest = 100)]
    public Property TotalExercise_Equals_SumExercise()
    {
        return Prop.ForAll(ArbDayEntries(), days =>
        {
            var result = StatisticsEndpoints.CalculateWeeklyKpi(days, 2025, 1);

            if (days.Count == 0)
            {
                return result.TotalExercise == 0;
            }

            var expected = days.SelectMany(d => d.Moments).Sum(m => m.Exercise);
            return result.TotalExercise == expected;
        });
    }

    /// <summary>
    /// Feature: statistics-kpi-dashboard, Property 3: Redondeo a un decimal
    /// Validates: Requirements 2.3
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AvgFood_IsRoundedToOneDecimal()
    {
        return Prop.ForAll(ArbDayEntries(), days =>
        {
            var result = StatisticsEndpoints.CalculateWeeklyKpi(days, 2025, 1);
            return Math.Round(result.AvgFood, 1) == result.AvgFood;
        });
    }
}
