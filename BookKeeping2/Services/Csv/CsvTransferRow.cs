namespace BookKeeping2.Services.Csv;

/// <summary>
/// Represents one transfer CSV row.
/// </summary>
public sealed class CsvTransferRow
{
    /// <summary>
    /// Gets or sets the one-based CSV row number.
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// Gets or sets the transfer date text.
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the currency text.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the amount text.
    /// </summary>
    public string Amount { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source account name.
    /// </summary>
    public string FromAccount { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the destination account name.
    /// </summary>
    public string ToAccount { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional note.
    /// </summary>
    public string? Note { get; set; }
}
