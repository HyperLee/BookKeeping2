using BookKeeping2.Localization;
using BookKeeping2.Models.Common;

namespace BookKeeping2.ViewModels.Transactions;

/// <summary>
/// Represents one income, expense, or transfer row in the transaction timeline.
/// </summary>
public sealed class TransactionTimelineItemViewModel
{
    /// <summary>
    /// Gets or sets the source record identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the record kind: Income, Expense, or Transfer.
    /// </summary>
    public string RecordKind { get; set; } = "Expense";

    /// <summary>
    /// Gets or sets the timeline date.
    /// </summary>
    public DateOnly TransactionDate { get; set; }

    /// <summary>
    /// Gets or sets the transaction type for income and expense rows.
    /// </summary>
    public TransactionType? Type { get; set; }

    /// <summary>
    /// Gets the display-only row type label.
    /// </summary>
    public string TypeText => RecordKind == "Transfer"
        ? "轉帳"
        : SystemDisplayLocalizer.GetTransactionTypeText(Type ?? TransactionType.Expense);

    /// <summary>
    /// Gets or sets the positive display amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency code.
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
    /// Gets or sets the category name. Transfer rows leave this empty.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account display text.
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source account name for transfer rows.
    /// </summary>
    public string? FromAccountName { get; set; }

    /// <summary>
    /// Gets or sets the destination account name for transfer rows.
    /// </summary>
    public string? ToAccountName { get; set; }

    /// <summary>
    /// Gets transfer direction text, or empty text for income and expense rows.
    /// </summary>
    public string TransferDirectionText => RecordKind == "Transfer"
        ? $"{FromAccountName} -> {ToAccountName}"
        : string.Empty;

    /// <summary>
    /// Gets or sets the optional note.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets the Razor Page path used to edit this row.
    /// </summary>
    public string EditPage { get; set; } = "/Transactions/Edit";

    /// <summary>
    /// Gets or sets the Razor Page path used to delete this row.
    /// </summary>
    public string DeletePage { get; set; } = "/Transactions/Delete";
}
