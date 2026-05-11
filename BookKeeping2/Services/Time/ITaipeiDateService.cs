namespace BookKeeping2.Services.Time;

/// <summary>
/// Provides current date and time values using the Asia/Taipei calendar boundary.
/// </summary>
public interface ITaipeiDateService
{
    /// <summary>
    /// Gets the current Asia/Taipei local date.
    /// </summary>
    DateOnly Today { get; }

    /// <summary>
    /// Gets the current Asia/Taipei local timestamp.
    /// </summary>
    DateTimeOffset NowTaipei { get; }

    /// <summary>
    /// Gets the current UTC timestamp.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
