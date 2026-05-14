using BookKeeping2.Models.Accounts;

namespace BookKeeping2.Services.Accounts;

/// <summary>
/// Manages accounts and balances.
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Creates an account.
    /// </summary>
    /// <param name="name">The account name.</param>
    /// <param name="type">The account type.</param>
    /// <param name="openingBalance">The opening balance.</param>
    /// <param name="currency">The supported account currency code.</param>
    /// <param name="iconKey">The icon key.</param>
    /// <param name="displayOrder">The display order.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The command result.</returns>
    Task<AccountResult> CreateAsync(
        string name,
        AccountType type,
        decimal openingBalance,
        string? currency,
        string iconKey = "wallet",
        int displayOrder = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates editable account fields while preserving the fixed account currency.
    /// </summary>
    /// <param name="id">The account identifier.</param>
    /// <param name="name">The account name.</param>
    /// <param name="type">The account type.</param>
    /// <param name="openingBalance">The opening balance.</param>
    /// <param name="currency">The supported account currency code that must match the existing account.</param>
    /// <param name="iconKey">The icon key.</param>
    /// <param name="displayOrder">The display order.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The command result.</returns>
    Task<AccountResult> UpdateAsync(
        long id,
        string name,
        AccountType type,
        decimal openingBalance,
        string? currency,
        string iconKey = "wallet",
        int displayOrder = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets account balance summaries.
    /// </summary>
    /// <param name="includeArchived">Whether archived rows should be included.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Balance summaries.</returns>
    Task<IReadOnlyList<AccountBalanceSummary>> GetBalanceSummariesAsync(bool includeArchived = true, CancellationToken cancellationToken = default);
}
