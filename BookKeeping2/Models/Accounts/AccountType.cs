namespace BookKeeping2.Models.Accounts;

/// <summary>
/// Represents a supported personal bookkeeping account category.
/// </summary>
public enum AccountType
{
    /// <summary>
    /// Cash on hand.
    /// </summary>
    Cash = 1,

    /// <summary>
    /// Bank account.
    /// </summary>
    Bank = 2,

    /// <summary>
    /// Credit card account.
    /// </summary>
    CreditCard = 3,

    /// <summary>
    /// Electronic wallet account.
    /// </summary>
    EWallet = 4,

    /// <summary>
    /// Other account type.
    /// </summary>
    Other = 99
}
