using System.ComponentModel.DataAnnotations.Schema;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Services.Common;

namespace BookKeeping2.Models.Transactions;

/// <summary>
/// Represents an income or expense transaction.
/// </summary>
public sealed class Transaction
{
    /// <summary>
    /// Gets or sets the database identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the Asia/Taipei local transaction date.
    /// </summary>
    public DateOnly TransactionDate { get; set; }

    /// <summary>
    /// Gets or sets the transaction type.
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Gets or sets the TWD amount as a decimal value.
    /// </summary>
    [NotMapped]
    public decimal Amount
    {
        get => MoneyMinorUnitConverter.FromMinorUnits(AmountMinorUnits);
        set => AmountMinorUnits = MoneyMinorUnitConverter.ToMinorUnits(value);
    }

    /// <summary>
    /// Gets or sets the TWD amount stored as minor units.
    /// </summary>
    public long AmountMinorUnits { get; set; }

    /// <summary>
    /// Gets or sets the category identifier.
    /// </summary>
    public long CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    public Category Category { get; set; } = null!;

    /// <summary>
    /// Gets or sets the account identifier.
    /// </summary>
    public long AccountId { get; set; }

    /// <summary>
    /// Gets or sets the account.
    /// </summary>
    public Account Account { get; set; } = null!;

    /// <summary>
    /// Gets or sets the sanitized optional note.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp in UTC.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the latest update timestamp in UTC.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets whether the transaction is soft deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the deletion timestamp in UTC.
    /// </summary>
    public DateTimeOffset? DeletedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the masked deletion summary.
    /// </summary>
    public string? DeletionSummary { get; set; }

    /// <summary>
    /// Gets or sets the masked latest change summary.
    /// </summary>
    public string LastChangeSummary { get; set; } = string.Empty;
}
