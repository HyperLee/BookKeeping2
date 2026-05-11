using BookKeeping2.ViewModels.Transactions;

namespace BookKeeping2.Services.Transactions;

/// <summary>
/// Searches and pages transaction lists.
/// </summary>
public interface ITransactionQueryService
{
    /// <summary>
    /// Searches non-deleted transactions with AND-combined filters.
    /// </summary>
    /// <param name="query">The search criteria.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The paged result.</returns>
    Task<PagedTransactionListViewModel> SearchAsync(TransactionQuery query, CancellationToken cancellationToken = default);
}
