namespace App.Api.Models;

/// <summary>
/// Provides the current local date based on a configured timezone.
/// Defaults to UTC if the timezone ID is not found.
/// </summary>
public class LocalClock
{
    private readonly TimeZoneInfo _tz;

    public LocalClock(string timeZoneId)
    {
        try { _tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); }
        catch { _tz = TimeZoneInfo.Utc; }
    }

    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _tz));
    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _tz);
}
