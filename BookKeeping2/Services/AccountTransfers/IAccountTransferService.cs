using BookKeeping2.ViewModels.AccountTransfers;

namespace BookKeeping2.Services.AccountTransfers;

/// <summary>
/// Manages account transfer creation, updates, soft deletion and form data.
/// </summary>
public interface IAccountTransferService
{
    /// <summary>
    /// Gets a visible transfer detail row.
    /// </summary>
    /// <param name="id">The transfer identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The transfer details, or null when not found.</returns>
    Task<AccountTransferDetailsViewModel?> GetDetailsAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an input model for editing.
    /// </summary>
    /// <param name="id">The transfer identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The edit input model, or null when not found.</returns>
    Task<AccountTransferInputModel?> GetForEditAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets form options for active accounts.
    /// </summary>
    /// <param name="currency">The selected currency code.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Form options.</returns>
    Task<AccountTransferFormOptionsViewModel> GetFormOptionsAsync(string? currency = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an account transfer.
    /// </summary>
    /// <param name="input">The transfer input.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The command result.</returns>
    Task<AccountTransferResult> CreateAsync(AccountTransferInputModel input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an account transfer.
    /// </summary>
    /// <param name="id">The transfer identifier.</param>
    /// <param name="input">The transfer input.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The command result.</returns>
    Task<AccountTransferResult> UpdateAsync(long id, AccountTransferInputModel input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes an account transfer.
    /// </summary>
    /// <param name="id">The transfer identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The command result.</returns>
    Task<AccountTransferResult> SoftDeleteAsync(long id, CancellationToken cancellationToken = default);
}
