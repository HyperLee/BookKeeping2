using BookKeeping2.Services.Time;

namespace BookKeeping2.Tests.TestSupport;

/// <summary>
/// Provides a fixed Asia/Taipei date for deterministic tests.
/// </summary>
public sealed class FakeTaipeiDateService : ITaipeiDateService
{
    /// <summary>
    /// Initializes a fake date service with the supplied local date.
    /// </summary>
    /// <param name="today">The date to expose as today.</param>
    public FakeTaipeiDateService(DateOnly today)
    {
        Today = today;
    }

    /// <summary>
    /// Gets or sets the current Asia/Taipei local date used by tests.
    /// </summary>
    public DateOnly Today { get; set; }

    /// <summary>
    /// Gets a fixed current timestamp for tests.
    /// </summary>
    public DateTimeOffset NowTaipei => new(Today, TimeOnly.MinValue, TimeSpan.FromHours(8));

    /// <summary>
    /// Gets a fixed UTC timestamp for persisted audit fields.
    /// </summary>
    public DateTimeOffset UtcNow => NowTaipei.ToUniversalTime();
}
