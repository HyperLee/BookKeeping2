namespace BookKeeping2.Services.Csv;

/// <summary>
/// Represents a transaction row in the fixed seven-column CSV contract.
/// </summary>
public sealed class CsvTransactionRow
{
    /// <summary>
    /// Gets or sets the transaction date in yyyy-MM-dd format.
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Traditional Chinese transaction type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the supported transaction currency code.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invariant decimal amount text.
    /// </summary>
    public string Amount { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exported category name.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exported account name.
    /// </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exported note.
    /// </summary>
    public string Note { get; set; } = string.Empty;
}
