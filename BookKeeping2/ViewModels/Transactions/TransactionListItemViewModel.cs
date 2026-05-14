using BookKeeping2.Localization;
using BookKeeping2.Models.Common;

namespace BookKeeping2.ViewModels.Transactions;

/// <summary>
/// Represents one row in the transaction list.
/// </summary>
public sealed class TransactionListItemViewModel
{
    /// <summary>
    /// Gets or sets the transaction identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the transaction date.
    /// </summary>
    public DateOnly TransactionDate { get; set; }

    /// <summary>
    /// Gets or sets the transaction type.
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Gets the display-only transaction type label.
    /// </summary>
    public string TypeText => SystemDisplayLocalizer.GetTransactionTypeText(Type);

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the transaction currency code.
    /// </summary>
    public string Currency { get; set; } = SupportedCurrency.LegacyDefaultCode;

    /// <summary>
    /// Gets the display-only currency name.
    /// </summary>
    public string CurrencyDisplayName => SupportedCurrency.GetDisplayName(Currency);

    /// <summary>
    /// Gets the display-only amount text with adjacent currency code.
    /// </summary>
    public string AmountText => $"{Currency} {Amount:N2}";

    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account name.
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional note.
    /// </summary>
    public string? Note { get; set; }
}
