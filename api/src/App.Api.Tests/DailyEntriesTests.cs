using App.Api.Endpoints;
using App.Api.Models;
using Xunit;

namespace App.Api.Tests;

public class DailyEntriesTests
{
    [Fact]
    public void MapToDailyEntryItem_CalculatesTotalExercise()
    {
        var entry = new DayEntry
        {
            Id = Guid.NewGuid(),
            Date = new DateOnly(2026, 3, 27),
            IsFinalized = true,
            Moments = [
                new() { Id = Guid.NewGuid(), Moment = "Mañana", Food = 3, Exercise = 2 },
                new() { Id = Guid.NewGuid(), Moment = "Mediodía", Food = 5, Exercise = 3 },
                new() { Id = Guid.NewGuid(), Moment = "Tarde", Food = 1, Exercise = 1 },
                new() { Id = Guid.NewGuid(), Moment = "Noche", Food = 2, Exercise = 5 },
            ]
        };

        var result = TrackerEndpoints.MapToDailyEntryItem(entry);

        Assert.Equal(11, result.TotalExercise); // 2+3+1+5
    }

    [Fact]
    public void MapToDailyEntryItem_MissingMoments_ReturnsZeroFood()
    {
        var entry = new DayEntry
        {
            Id = Guid.NewGuid(),
            Date = new DateOnly(2026, 3, 27),
            IsFinalized = true,
            Moments = [
                new() { Id = Guid.NewGuid(), Moment = "Mañana", Food = 5, Exercise = 0 },
            ]
        };

        var result = TrackerEndpoints.MapToDailyEntryItem(entry);

        Assert.Equal(5, result.FoodMañana);
        Assert.Equal(0, result.FoodMediodia);
        Assert.Equal(0, result.FoodTarde);
        Assert.Equal(0, result.FoodNoche);
    }

    [Theory]
    [InlineData(0, 10, 1, 10)]    // page < 1 → 1
    [InlineData(-5, 10, 1, 10)]   // page < 1 → 1
    [InlineData(1, 0, 1, 10)]     // pageSize < 1 → 10
    [InlineData(1, 51, 1, 10)]    // pageSize > 50 → 10
    [InlineData(1, -1, 1, 10)]    // pageSize < 1 → 10
    [InlineData(2, 25, 2, 25)]    // valid values unchanged
    [InlineData(1, 50, 1, 50)]    // boundary valid
    [InlineData(1, 1, 1, 1)]      // boundary valid
    public void NormalizePaginationParams_CorrectlyNormalizes(
        int inputPage, int inputPageSize,
        int expectedPage, int expectedPageSize)
    {
        var (page, pageSize) = TrackerEndpoints.NormalizePaginationParams(inputPage, inputPageSize);

        Assert.Equal(expectedPage, page);
        Assert.Equal(expectedPageSize, pageSize);
    }
}
