namespace BookKeeping2.Models.CsvImports;

/// <summary>
/// Represents one failed CSV row.
/// </summary>
public sealed class CsvImportError
{
    /// <summary>
    /// Gets or sets the database identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the parent import batch identifier.
    /// </summary>
    public long CsvImportBatchId { get; set; }

    /// <summary>
    /// Gets or sets the parent import batch.
    /// </summary>
    public CsvImportBatch CsvImportBatch { get; set; } = null!;

    /// <summary>
    /// Gets or sets the one-based CSV row number.
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// Gets or sets the field name associated with the error.
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// Gets or sets the Traditional Chinese error reason.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a masked and truncated raw value preview.
    /// </summary>
    public string? RawValuePreview { get; set; }
}
