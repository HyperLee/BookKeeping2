using BookKeeping2.Localization;
using BookKeeping2.Models.Accounts;

namespace BookKeeping2.Services.Accounts;

/// <summary>
/// Represents an account balance summary.
/// </summary>
public sealed class AccountBalanceSummary
{
    /// <summary>
    /// Gets or sets the account identifier.
    /// </summary>
    public long AccountId { get; set; }

    /// <summary>
    /// Gets or sets the account name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account type.
    /// </summary>
    public AccountType Type { get; set; }

    /// <summary>
    /// Gets the display-only account type label.
    /// </summary>
    public string TypeText => SystemDisplayLocalizer.GetAccountTypeText(Type);

    /// <summary>
    /// Gets or sets the current balance.
    /// </summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>
    /// Gets or sets whether the account is archived.
    /// </summary>
    public bool IsArchived { get; set; }
}
