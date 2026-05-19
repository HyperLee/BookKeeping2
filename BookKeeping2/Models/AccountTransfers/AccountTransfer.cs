using System.ComponentModel.DataAnnotations.Schema;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Common;
using BookKeeping2.Services.Common;

namespace BookKeeping2.Models.AccountTransfers;

/// <summary>
/// Represents a money movement between two accounts with the same currency.
/// </summary>
/// <remarks>
/// The persisted amount uses integer minor units while the domain-facing
/// <see cref="Amount" /> property uses <see cref="decimal" />.
/// <example>
/// <code>
/// var transfer = new AccountTransfer
/// {
///     Currency = "TWD",
///     Amount = 1000m,
///     FromAccountId = bank.Id,
///     ToAccountId = cash.Id
/// };
/// </code>
/// </example>
/// </remarks>
public sealed class AccountTransfer
{
    /// <summary>
    /// Gets or sets the database identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the Asia/Taipei local transfer date.
    /// </summary>
    public DateOnly TransferDate { get; set; }

    /// <summary>
    /// Gets or sets the uppercase supported currency code for this transfer.
    /// </summary>
    public string Currency { get; set; } = SupportedCurrency.LegacyDefaultCode;

    /// <summary>
    /// Gets or sets the transfer amount as a decimal value in <see cref="Currency" />.
    /// </summary>
    [NotMapped]
    public decimal Amount
    {
        get => MoneyMinorUnitConverter.FromMinorUnits(AmountMinorUnits);
        set => AmountMinorUnits = MoneyMinorUnitConverter.ToMinorUnits(value);
    }

    /// <summary>
    /// Gets or sets the transfer amount stored as minor units.
    /// </summary>
    public long AmountMinorUnits { get; set; }

    /// <summary>
    /// Gets or sets the source account identifier.
    /// </summary>
    public long FromAccountId { get; set; }

    /// <summary>
    /// Gets or sets the source account.
    /// </summary>
    public Account FromAccount { get; set; } = null!;

    /// <summary>
    /// Gets or sets the destination account identifier.
    /// </summary>
    public long ToAccountId { get; set; }

    /// <summary>
    /// Gets or sets the destination account.
    /// </summary>
    public Account ToAccount { get; set; } = null!;

    /// <summary>
    /// Gets or sets the sanitized optional note.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets the opaque one-time form submission token.
    /// </summary>
    public string SubmissionToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp in UTC.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the latest update timestamp in UTC.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets whether the transfer is soft deleted.
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
