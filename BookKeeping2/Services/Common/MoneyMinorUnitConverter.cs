namespace BookKeeping2.Services.Common;

/// <summary>
/// Converts TWD decimal amounts to and from integer minor units.
/// </summary>
public static class MoneyMinorUnitConverter
{
    /// <summary>
    /// Maximum supported transaction amount in TWD.
    /// </summary>
    public const decimal MaximumTransactionAmount = 999_999_999.99m;

    /// <summary>
    /// Converts a TWD amount into cents.
    /// </summary>
    /// <param name="amount">The decimal amount.</param>
    /// <param name="requirePositive">Whether the amount must be greater than zero.</param>
    /// <returns>The amount represented as integer minor units.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when amount violates range rules.</exception>
    /// <exception cref="OverflowException">Thrown when conversion would overflow a 64-bit integer.</exception>
    public static long ToMinorUnits(decimal amount, bool requirePositive = true)
    {
        if (requirePositive && amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "金額必須大於 0。");
        }

        if (Math.Abs(amount) > MaximumTransactionAmount)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "金額不可超過 TWD 999,999,999.99。");
        }

        decimal scaled = amount * 100m;
        if (decimal.Truncate(scaled) != scaled)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "金額最多只能有 2 位小數。");
        }

        if (scaled > long.MaxValue || scaled < long.MinValue)
        {
            throw new OverflowException("金額超過可儲存範圍。");
        }

        return decimal.ToInt64(scaled);
    }

    /// <summary>
    /// Converts integer minor units into a TWD decimal amount.
    /// </summary>
    /// <param name="minorUnits">The amount represented as cents.</param>
    /// <returns>The decimal amount.</returns>
    public static decimal FromMinorUnits(long minorUnits)
    {
        return minorUnits / 100m;
    }
}
