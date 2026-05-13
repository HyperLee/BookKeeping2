using System.Globalization;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Common;
using BookKeeping2.Services.Budgets;

namespace BookKeeping2.Localization;

/// <summary>
/// Provides display-only labels for system-defined values without changing persisted data.
/// </summary>
public static class SystemDisplayLocalizer
{
    private static readonly IReadOnlyDictionary<string, string> EnglishDefaultCategories = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["餐飲"] = "Food & Dining",
        ["交通"] = "Transportation",
        ["娛樂"] = "Entertainment",
        ["購物"] = "Shopping",
        ["居住"] = "Housing",
        ["醫療"] = "Healthcare",
        ["教育"] = "Education",
        ["其他"] = "Other",
        ["薪資"] = "Salary",
        ["獎金"] = "Bonus",
        ["投資收益"] = "Investment Income",
        ["其他收入"] = "Other Income"
    };

    /// <summary>
    /// Gets the display text for a transaction type.
    /// </summary>
    /// <param name="type">The transaction type.</param>
    /// <returns>A localized display label.</returns>
    public static string GetTransactionTypeText(TransactionType type)
    {
        if (IsEnglishUi())
        {
            return type == TransactionType.Income ? "Income" : "Expense";
        }

        return type == TransactionType.Income ? "收入" : "支出";
    }

    /// <summary>
    /// Gets the display text for an account type.
    /// </summary>
    /// <param name="type">The account type.</param>
    /// <returns>A localized display label.</returns>
    public static string GetAccountTypeText(AccountType type)
    {
        return type switch
        {
            AccountType.Bank => IsEnglishUi() ? "Bank" : "銀行",
            AccountType.CreditCard => IsEnglishUi() ? "Credit Card" : "信用卡",
            AccountType.EWallet => IsEnglishUi() ? "E-Wallet" : "電子支付",
            AccountType.Other => IsEnglishUi() ? "Other" : "其他",
            _ => IsEnglishUi() ? "Cash" : "現金"
        };
    }

    /// <summary>
    /// Gets the display text for a default category name.
    /// </summary>
    /// <param name="categoryName">The persisted category name.</param>
    /// <param name="isDefault">Whether the category is system-defined seed data.</param>
    /// <returns>A display-only category name.</returns>
    public static string GetCategoryName(string categoryName, bool isDefault)
    {
        if (isDefault && IsEnglishUi() && EnglishDefaultCategories.TryGetValue(categoryName, out string? englishName))
        {
            return englishName;
        }

        return categoryName;
    }

    /// <summary>
    /// Gets the display text for a budget alert state.
    /// </summary>
    /// <param name="state">The alert state.</param>
    /// <param name="overspentAmount">The overspent amount, if any.</param>
    /// <returns>A localized display label.</returns>
    public static string GetBudgetAlertText(BudgetAlertState state, decimal overspentAmount)
    {
        return state switch
        {
            BudgetAlertState.Exceeded => IsEnglishUi() ? $"Over budget by {overspentAmount:N0}" : $"已超出預算 {overspentAmount:N0}",
            BudgetAlertState.NearLimit => IsEnglishUi() ? "Near budget limit" : "接近預算上限",
            _ => IsEnglishUi() ? "Usage normal" : "使用率正常"
        };
    }

    private static bool IsEnglishUi()
    {
        return string.Equals(CultureInfo.CurrentUICulture.Name, UiLanguageOptions.EnglishUiCultureName, StringComparison.OrdinalIgnoreCase);
    }
}
