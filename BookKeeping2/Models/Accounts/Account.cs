using System.ComponentModel.DataAnnotations.Schema;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Services.Common;

namespace BookKeeping2.Models.Accounts;

/// <summary>
/// Represents an account with a fixed currency used by transactions.
/// </summary>
public sealed class Account
{
    /// <summary>
    /// Gets or sets the database identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the normalized unique name.
    /// </summary>
    public string NormalizedName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account type.
    /// </summary>
    public AccountType Type { get; set; }

    /// <summary>
    /// Gets or sets the safe icon key.
    /// </summary>
    public string IconKey { get; set; } = "wallet";

    /// <summary>
    /// Gets or sets the opening balance as a decimal value in <see cref="Currency" />.
    /// </summary>
    [NotMapped]
    public decimal OpeningBalance
    {
        get => MoneyMinorUnitConverter.FromMinorUnits(OpeningBalanceMinorUnits);
        set => OpeningBalanceMinorUnits = MoneyMinorUnitConverter.ToMinorUnits(value, requirePositive: false);
    }

    /// <summary>
    /// Gets or sets the opening balance in minor units.
    /// </summary>
    public long OpeningBalanceMinorUnits { get; set; }

    /// <summary>
    /// Gets or sets the fixed uppercase supported currency code.
    /// </summary>
    public string Currency { get; set; } = SupportedCurrency.LegacyDefaultCode;

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets whether this account is hidden from new selections.
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp in UTC.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the latest update timestamp in UTC.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets transactions that reference this account.
    /// </summary>
    public ICollection<Transaction> Transactions { get; } = new List<Transaction>();
}
