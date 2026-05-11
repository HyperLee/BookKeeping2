using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Settings;

namespace BookKeeping2.Data.SeedData;

/// <summary>
/// Provides built-in data required for first launch.
/// </summary>
public static class DefaultSeedData
{
    /// <summary>
    /// Gets the default expense category names.
    /// </summary>
    public static readonly string[] ExpenseCategoryNames =
    [
        "餐飲",
        "交通",
        "娛樂",
        "購物",
        "居住",
        "醫療",
        "教育",
        "其他"
    ];

    /// <summary>
    /// Gets the default income category names.
    /// </summary>
    public static readonly string[] IncomeCategoryNames =
    [
        "薪資",
        "獎金",
        "投資收益",
        "其他收入"
    ];

    /// <summary>
    /// Creates missing default categories.
    /// </summary>
    /// <param name="nowUtc">The timestamp to apply to seed rows.</param>
    /// <returns>Default category rows.</returns>
    public static IEnumerable<Category> CreateCategories(DateTimeOffset nowUtc)
    {
        int displayOrder = 1;
        foreach (string name in ExpenseCategoryNames)
        {
            yield return CreateCategory(name, TransactionType.Expense, displayOrder++, nowUtc);
        }

        displayOrder = 1;
        foreach (string name in IncomeCategoryNames)
        {
            yield return CreateCategory(name, TransactionType.Income, displayOrder++, nowUtc);
        }
    }

    /// <summary>
    /// Creates default non-secret application settings.
    /// </summary>
    /// <param name="nowUtc">The timestamp to apply to seed rows.</param>
    /// <returns>Default application settings.</returns>
    public static IEnumerable<AppSetting> CreateSettings(DateTimeOffset nowUtc)
    {
        yield return new AppSetting { Key = "Currency", Value = "TWD", UpdatedAtUtc = nowUtc };
        yield return new AppSetting { Key = "TimeZone", Value = "Asia/Taipei", UpdatedAtUtc = nowUtc };
        yield return new AppSetting { Key = "BudgetWarningThreshold", Value = "0.8", UpdatedAtUtc = nowUtc };
    }

    /// <summary>
    /// Normalizes a user-visible name for uniqueness checks.
    /// </summary>
    /// <param name="name">The source name.</param>
    /// <returns>The normalized name.</returns>
    public static string NormalizeName(string name)
    {
        return name.Trim().ToUpperInvariant();
    }

    private static Category CreateCategory(string name, TransactionType type, int displayOrder, DateTimeOffset nowUtc)
    {
        return new Category
        {
            Name = name,
            NormalizedName = NormalizeName(name),
            Type = type,
            IconKey = type == TransactionType.Expense ? "tag" : "cash-coin",
            DisplayOrder = displayOrder,
            IsDefault = true,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        };
    }
}
