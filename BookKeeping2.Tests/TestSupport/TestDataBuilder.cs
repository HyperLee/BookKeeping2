using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Budgets;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;

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

    /// <summary>
    /// Creates a test category.
    /// </summary>
    /// <param name="type">The category transaction type.</param>
    /// <param name="name">The category name.</param>
    /// <returns>A test category instance.</returns>
    public static Category CreateCategory(TransactionType type, string name = "餐飲")
    {
        return new Category
        {
            Name = name,
            NormalizedName = name.Trim().ToUpperInvariant(),
            Type = type,
            IconKey = "tag",
            DisplayOrder = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a test account with a specified currency.
    /// </summary>
    /// <param name="name">The account name.</param>
    /// <param name="currency">The account currency code.</param>
    /// <returns>A test account instance.</returns>
    public static Account CreateAccount(string name = "現金", string currency = TwdCurrency)
    {
        return new Account
        {
            Name = name,
            NormalizedName = name.Trim().ToUpperInvariant(),
            Type = AccountType.Cash,
            IconKey = "wallet",
            Currency = currency,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a pair of active accounts suitable for account transfer tests.
    /// </summary>
    /// <param name="currency">The currency shared by both accounts.</param>
    /// <returns>The source and destination accounts.</returns>
    public static (Account FromAccount, Account ToAccount) CreateTransferAccountPair(string currency = TwdCurrency)
    {
        return (
            CreateAccount("銀行", currency),
            CreateAccount("現金", currency));
    }

    /// <summary>
    /// Creates a stable submission token for account transfer form tests.
    /// </summary>
    /// <param name="suffix">The token suffix used to distinguish scenarios.</param>
    /// <returns>A test submission token.</returns>
    public static string CreateTransferSubmissionToken(string suffix = "default")
    {
        return $"transfer-token-{suffix}";
    }

    /// <summary>
    /// Creates a test transaction with a specified currency.
    /// </summary>
    /// <param name="category">The transaction category.</param>
    /// <param name="account">The transaction account.</param>
    /// <param name="currency">The transaction currency code.</param>
    /// <returns>A test transaction instance.</returns>
    public static Transaction CreateTransaction(Category category, Account account, string currency = TwdCurrency)
    {
        return new Transaction
        {
            TransactionDate = DefaultToday,
            Type = category.Type,
            Amount = 100m,
            Currency = currency,
            CategoryId = category.Id,
            Category = category,
            AccountId = account.Id,
            Account = account,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = $"{currency} transaction"
        };
    }

    /// <summary>
    /// Creates a test budget with a specified currency.
    /// </summary>
    /// <param name="category">The expense category.</param>
    /// <param name="currency">The budget currency code.</param>
    /// <returns>A test budget instance.</returns>
    public static Budget CreateBudget(Category category, string currency = TwdCurrency)
    {
        return new Budget
        {
            CategoryId = category.Id,
            Category = category,
            BudgetMonth = new DateOnly(DefaultToday.Year, DefaultToday.Month, 1),
            Amount = 1000m,
            Currency = currency,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
    }
}
