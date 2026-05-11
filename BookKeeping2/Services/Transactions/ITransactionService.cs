using BookKeeping2.Models.Common;
using BookKeeping2.ViewModels.Transactions;

namespace BookKeeping2.Services.Transactions;

/// <summary>
/// Manages transaction creation, updates, soft deletion and list queries.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Lists non-deleted transactions.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Visible transaction rows.</returns>
    Task<IReadOnlyList<TransactionListItemViewModel>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a transaction row by identifier.
    /// </summary>
    /// <param name="id">The transaction identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The transaction row, or null when not found.</returns>
    Task<TransactionListItemViewModel?> GetDetailsAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an input model for editing.
    /// </summary>
    /// <param name="id">The transaction identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The edit input model, or null when not found.</returns>
    Task<TransactionInputModel?> GetForEditAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets form options for the selected type.
    /// </summary>
    /// <param name="type">The selected transaction type.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Form options.</returns>
    Task<TransactionFormOptionsViewModel> GetFormOptionsAsync(TransactionType? type = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a transaction.
    /// </summary>
    /// <param name="input">The transaction input.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The command result.</returns>
    Task<TransactionResult> CreateAsync(TransactionInputModel input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a transaction.
    /// </summary>
    /// <param name="id">The transaction identifier.</param>
    /// <param name="input">The transaction input.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The command result.</returns>
    Task<TransactionResult> UpdateAsync(long id, TransactionInputModel input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a transaction.
    /// </summary>
    /// <param name="id">The transaction identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The command result.</returns>
    Task<TransactionResult> SoftDeleteAsync(long id, CancellationToken cancellationToken = default);
}
