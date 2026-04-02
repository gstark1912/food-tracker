namespace App.Api.Models;

public record MomentEntryDto(string Moment, int Food, int Exercise);

public record SaveDayRequest(List<MomentEntryDto> Moments);

public record NextPendingResponse(
    string Date,
    bool IsCurrentDay,
    bool IsFinalized,
    List<MomentEntryDto> Moments
);

public record FinalizeDayResponse(string? NextPendingDate);

public record RegisterWeightRequest(decimal WeightKg);

public record WeeklySummaryResponse(
    int Year,
    int WeekNumber,
    int WeeklyScore,
    decimal? WeightKg,
    string WeekStart,
    string WeekEnd
);

public record WeeklySummaryWithTrend(
    WeeklySummaryResponse Summary,
    string? Trend
);

public record StatisticsResponse(List<WeeklySummaryWithTrend> Summaries);

public record WeeklyKpiData(
    int Year,
    int WeekNumber,
    decimal AvgFood,
    int TotalExercise,
    int FinalizedDays
);

public record WeeklyKpiResponse(
    WeeklyKpiData CurrentWeek,
    WeeklyKpiData? PreviousWeek
);
