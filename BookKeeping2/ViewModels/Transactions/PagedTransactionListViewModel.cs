namespace BookKeeping2.ViewModels.Transactions;

/// <summary>
/// Represents a page of transaction list items.
/// </summary>
public sealed class PagedTransactionListViewModel
{
    /// <summary>
    /// Gets or sets the current page items.
    /// </summary>
    public IReadOnlyList<TransactionListItemViewModel> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the total matching item count.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the one-based current page.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => TotalCount == 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Gets whether a previous page exists.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Gets whether a next page exists.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}
