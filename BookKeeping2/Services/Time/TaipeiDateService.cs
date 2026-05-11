namespace BookKeeping2.Services.Time;

/// <summary>
/// Gets current time values using the Asia/Taipei time zone.
/// </summary>
public sealed class TaipeiDateService : ITaipeiDateService
{
    private static readonly TimeZoneInfo TaipeiTimeZone = ResolveTaipeiTimeZone();

    /// <inheritdoc />
    public DateOnly Today => DateOnly.FromDateTime(NowTaipei.DateTime);

    /// <inheritdoc />
    public DateTimeOffset NowTaipei => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TaipeiTimeZone);

    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    private static TimeZoneInfo ResolveTaipeiTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei");
        }
    }
}
