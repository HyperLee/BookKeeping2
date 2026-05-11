namespace BookKeeping2.Tests.TestSupport;

/// <summary>
/// Centralizes small reusable values used by bookkeeping tests.
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// A stable Asia/Taipei date used by tests unless a scenario needs another date.
    /// </summary>
    public static readonly DateOnly DefaultToday = new(2026, 5, 11);

    /// <summary>
    /// Creates a fake date service fixed to the repository feature date.
    /// </summary>
    /// <returns>A fake date service for deterministic tests.</returns>
    public static FakeTaipeiDateService CreateDateService()
    {
        return new FakeTaipeiDateService(DefaultToday);
    }
}
