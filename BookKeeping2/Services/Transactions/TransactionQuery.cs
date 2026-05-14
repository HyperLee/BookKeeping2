namespace BookKeeping2.Services.Transactions;

/// <summary>
/// Represents transaction search criteria.
/// </summary>
public sealed class TransactionQuery
{
    /// <summary>
    /// Gets or sets the optional inclusive start date.
    /// </summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the optional inclusive end date.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the optional category identifier.
    /// </summary>
    public long? CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the optional account identifier.
    /// </summary>
    public long? AccountId { get; set; }

    /// <summary>
    /// Gets or sets the optional supported currency code.
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Gets or sets the optional minimum amount.
    /// </summary>
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// Gets or sets the optional maximum amount.
    /// </summary>
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// Gets or sets the optional keyword.
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// Gets or sets the one-based page.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; } = 50;
}
