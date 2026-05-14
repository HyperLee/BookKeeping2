namespace BookKeeping2.Models.Common;

/// <summary>
/// Provides the fixed supported currency catalog for bookkeeping records.
/// </summary>
public static class SupportedCurrency
{
    /// <summary>
    /// Gets the legacy default currency code used for existing data without an explicit currency.
    /// </summary>
    public const string LegacyDefaultCode = "TWD";

    private static readonly SupportedCurrencyOption[] supportedOptions =
    [
        new("TWD", "新台幣", 1),
        new("USD", "美金", 2),
        new("JPY", "日幣", 3),
        new("EUR", "歐元", 4),
        new("GBP", "英鎊", 5)
    ];

    private static readonly Dictionary<string, SupportedCurrencyOption> optionsByCode =
        supportedOptions.ToDictionary(option => option.Code, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets supported currency options in UI display order.
    /// </summary>
    public static IReadOnlyList<SupportedCurrencyOption> Options => supportedOptions;

    /// <summary>
    /// Attempts to trim, validate and normalize a currency code.
    /// </summary>
    /// <param name="input">The untrusted currency code input.</param>
    /// <param name="normalizedCode">The uppercase supported currency code when validation succeeds.</param>
    /// <returns><see langword="true" /> when the input is a supported currency code; otherwise <see langword="false" />.</returns>
    public static bool TryNormalize(string? input, out string? normalizedCode)
    {
        string? trimmed = input?.Trim();
        if (string.IsNullOrEmpty(trimmed) || !optionsByCode.TryGetValue(trimmed, out SupportedCurrencyOption? option))
        {
            normalizedCode = null;
            return false;
        }

        normalizedCode = option.Code;
        return true;
    }

    /// <summary>
    /// Trims, validates and normalizes a supported currency code.
    /// </summary>
    /// <param name="input">The untrusted currency code input.</param>
    /// <returns>The uppercase supported currency code.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="input" /> is blank or unsupported.</exception>
    public static string Normalize(string? input)
    {
        if (TryNormalize(input, out string? normalizedCode))
        {
            return normalizedCode!;
        }

        throw new ArgumentException("Unsupported currency code.", nameof(input));
    }

    /// <summary>
    /// Gets the Traditional Chinese display name for a supported currency code.
    /// </summary>
    /// <param name="input">The untrusted currency code input.</param>
    /// <returns>The Traditional Chinese currency display name.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="input" /> is blank or unsupported.</exception>
    public static string GetDisplayName(string? input)
    {
        string normalizedCode = Normalize(input);
        return optionsByCode[normalizedCode].DisplayName;
    }
}

/// <summary>
/// Describes one supported bookkeeping currency option.
/// </summary>
public sealed class SupportedCurrencyOption
{
    /// <summary>
    /// Initializes a new supported currency option.
    /// </summary>
    /// <param name="code">The persisted uppercase currency code.</param>
    /// <param name="displayName">The Traditional Chinese display name.</param>
    /// <param name="sortOrder">The stable UI sort order.</param>
    public SupportedCurrencyOption(string code, string displayName, int sortOrder)
    {
        Code = code;
        DisplayName = displayName;
        SortOrder = sortOrder;
    }

    /// <summary>
    /// Gets the persisted uppercase currency code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the Traditional Chinese display name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the stable UI sort order.
    /// </summary>
    public int SortOrder { get; }
}
