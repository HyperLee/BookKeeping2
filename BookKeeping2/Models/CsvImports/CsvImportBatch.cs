namespace BookKeeping2.Models.CsvImports;

/// <summary>
/// Represents a CSV import batch summary.
/// </summary>
public sealed class CsvImportBatch
{
    /// <summary>
    /// Gets or sets the database identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the safe original file name.
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the import timestamp in UTC.
    /// </summary>
    public DateTimeOffset ImportedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the total processed data rows.
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// Gets or sets the number of rows that created transactions.
    /// </summary>
    public int SucceededRows { get; set; }

    /// <summary>
    /// Gets or sets the number of failed rows.
    /// </summary>
    public int FailedRows { get; set; }

    /// <summary>
    /// Gets or sets the number of categories created during import.
    /// </summary>
    public int CreatedCategoryCount { get; set; }

    /// <summary>
    /// Gets or sets the user-readable masked summary.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Gets row-level import errors.
    /// </summary>
    public ICollection<CsvImportError> Errors { get; } = new List<CsvImportError>();
}
