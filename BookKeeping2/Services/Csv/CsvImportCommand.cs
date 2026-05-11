namespace BookKeeping2.Services.Csv;

/// <summary>
/// Represents an uploaded CSV import file.
/// </summary>
/// <param name="FileName">The original file name supplied by the browser.</param>
/// <param name="Content">The uploaded CSV bytes.</param>
public sealed record CsvImportCommand(string FileName, byte[] Content);

/// <summary>
/// Represents one parsed CSV data row before domain validation.
/// </summary>
public sealed class CsvImportRow
{
    /// <summary>
    /// Gets or sets the one-based CSV row number.
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// Gets or sets the raw date text.
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw type text.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw amount text.
    /// </summary>
    public string Amount { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw category text.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw account text.
    /// </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw note text.
    /// </summary>
    public string Note { get; set; } = string.Empty;
}
