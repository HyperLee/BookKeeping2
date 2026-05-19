using BookKeeping2.ViewModels.Transactions;

namespace BookKeeping2.Services.Transactions;

/// <summary>
/// Searches and pages transaction timelines.
/// </summary>
public interface ITransactionQueryService
{
    /// <summary>
    /// Searches non-deleted income, expense, and transfer timeline rows with AND-combined filters.
    /// Category filters apply only to income and expense rows and exclude transfer rows.
    /// </summary>
    /// <param name="query">The search criteria.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The paged result.</returns>
    Task<PagedTransactionListViewModel> SearchAsync(TransactionQuery query, CancellationToken cancellationToken = default);
}
