namespace BookKeeping2.Tests.TestSupport;

/// <summary>
/// Centralizes small reusable values used by bookkeeping tests.
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// Test currency code for New Taiwan Dollar scenarios.
    /// </summary>
    public const string TwdCurrency = "TWD";

    /// <summary>
    /// Test currency code for United States Dollar scenarios.
    /// </summary>
    public const string UsdCurrency = "USD";

    /// <summary>
    /// Test currency code for Japanese Yen scenarios.
    /// </summary>
    public const string JpyCurrency = "JPY";

    /// <summary>
    /// Test currency code for Euro scenarios.
    /// </summary>
    public const string EurCurrency = "EUR";

    /// <summary>
    /// Test currency code for British Pound scenarios.
    /// </summary>
    public const string GbpCurrency = "GBP";

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
