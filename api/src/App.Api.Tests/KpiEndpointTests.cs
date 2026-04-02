using App.Api.Endpoints;
using App.Api.Models;
using Xunit;

namespace App.Api.Tests;

/// <summary>
/// Unit tests for StatisticsEndpoints.CalculateWeeklyKpi
/// </summary>
public class KpiEndpointTests
{
    [Fact]
    public void CalculateWeeklyKpi_EmptyList_ReturnsZeros()
    {
        var result = StatisticsEndpoints.CalculateWeeklyKpi([], 2025, 27);

        Assert.Equal(0m, result.AvgFood);
        Assert.Equal(0, result.TotalExercise);
        Assert.Equal(0, result.FinalizedDays);
        Assert.Equal(2025, result.Year);
        Assert.Equal(27, result.WeekNumber);
    }

    [Fact]
    public void CalculateWeeklyKpi_EmptyList_UsedForPreviousWeekNull()
    {
        // When there are no finalized days for a week, the endpoint returns null for previousWeek.
        // CalculateWeeklyKpi with empty list returns zeros and FinalizedDays=0,
        // which the endpoint uses (prevWeekDays.Count == 0) to decide previousWeek = null.
        var emptyDays = new List<DayEntry>();
        var result = StatisticsEndpoints.CalculateWeeklyKpi(emptyDays, 2025, 26);

        Assert.Equal(0m, result.AvgFood);
        Assert.Equal(0, result.TotalExercise);
        Assert.Equal(0, result.FinalizedDays);
    }

    [Fact]
    public void CalculateWeeklyKpi_WithSampleData_CalculatesCorrectly()
    {
        var day1Id = Guid.NewGuid();
        var day2Id = Guid.NewGuid();
        var day3Id = Guid.NewGuid();

        var days = new List<DayEntry>
        {
            new()
            {
                Id = day1Id,
                Date = new DateOnly(2025, 7, 7),
                IsFinalized = true,
                Moments =
                [
                    new() { Id = Guid.NewGuid(), DayEntryId = day1Id, Moment = "Desayuno", Food = 3, Exercise = 5 },
                    new() { Id = Guid.NewGuid(), DayEntryId = day1Id, Moment = "Almuerzo", Food = 5, Exercise = 0 }
                ]
            },
            new()
            {
                Id = day2Id,
                Date = new DateOnly(2025, 7, 8),
                IsFinalized = true,
                Moments =
                [
                    new() { Id = Guid.NewGuid(), DayEntryId = day2Id, Moment = "Desayuno", Food = 8, Exercise = 3 },
                    new() { Id = Guid.NewGuid(), DayEntryId = day2Id, Moment = "Cena", Food = 2, Exercise = 8 }
                ]
            },
            new()
            {
                Id = day3Id,
                Date = new DateOnly(2025, 7, 9),
                IsFinalized = true,
                Moments =
                [
                    new() { Id = Guid.NewGuid(), DayEntryId = day3Id, Moment = "Desayuno", Food = 1, Exercise = 2 }
                ]
            }
        };

        var result = StatisticsEndpoints.CalculateWeeklyKpi(days, 2025, 28);

        // totalFood = (3+5) + (8+2) + (1) = 19
        // avgFood = Math.Round(19 / 3, 1) = Math.Round(6.333..., 1) = 6.3
        Assert.Equal(Math.Round(19m / 3, 1, MidpointRounding.AwayFromZero), result.AvgFood);
        Assert.Equal(6.3m, result.AvgFood);

        // totalExercise = (5+0) + (3+8) + (2) = 18
        Assert.Equal(18, result.TotalExercise);

        Assert.Equal(3, result.FinalizedDays);
        Assert.Equal(2025, result.Year);
        Assert.Equal(28, result.WeekNumber);
    }
}
