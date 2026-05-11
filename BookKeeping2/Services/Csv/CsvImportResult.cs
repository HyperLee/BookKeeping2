namespace BookKeeping2.Services.Csv;

/// <summary>
/// Represents CSV import parsing and persistence results.
/// </summary>
public sealed class CsvImportResult
{
    /// <summary>
    /// Gets or sets the safe original file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets total processed data rows.
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// Gets or sets successful imported rows.
    /// </summary>
    public int SucceededRows { get; set; }

    /// <summary>
    /// Gets or sets failed rows.
    /// </summary>
    public int FailedRows { get; set; }

    /// <summary>
    /// Gets parsed rows that still need domain validation.
    /// </summary>
    public IList<CsvImportRow> Rows { get; } = [];

    /// <summary>
    /// Gets row or file level errors.
    /// </summary>
    public IList<CsvImportErrorDetail> Errors { get; } = [];

    /// <summary>
    /// Gets category names created during import.
    /// </summary>
    public IList<string> CreatedCategories { get; } = [];

    /// <summary>
    /// Gets or sets the user-readable summary.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Adds an error to the result.
    /// </summary>
    /// <param name="rowNumber">The one-based row number, or 0 for file-level errors.</param>
    /// <param name="fieldName">The optional field name.</param>
    /// <param name="reason">The Traditional Chinese reason.</param>
    /// <param name="rawValuePreview">The masked raw value preview.</param>
    public void AddError(int rowNumber, string? fieldName, string reason, string? rawValuePreview = null)
    {
        Errors.Add(new CsvImportErrorDetail
        {
            RowNumber = rowNumber,
            FieldName = fieldName,
            Reason = reason,
            RawValuePreview = rawValuePreview
        });
    }
}

/// <summary>
/// Represents one CSV import error.
/// </summary>
public sealed class CsvImportErrorDetail
{
    /// <summary>
    /// Gets or sets the one-based row number.
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// Gets or sets the optional field name.
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// Gets or sets the Traditional Chinese reason.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the masked raw value preview.
    /// </summary>
    public string? RawValuePreview { get; set; }
}
