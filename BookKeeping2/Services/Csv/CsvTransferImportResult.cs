namespace BookKeeping2.Services.Csv;

/// <summary>
/// Represents transfer CSV parsing and persistence results.
/// </summary>
public sealed class CsvTransferImportResult
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
    public IList<CsvTransferRow> Rows { get; } = [];

    /// <summary>
    /// Gets row or file level errors.
    /// </summary>
    public IList<CsvImportErrorDetail> Errors { get; } = [];

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
    /// <param name="rawValuePreview">The safe raw value preview.</param>
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
