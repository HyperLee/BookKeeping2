namespace BookKeeping2.Services.Csv;

/// <summary>
/// Options that constrain exported transactions.
/// </summary>
public sealed class CsvExportOptions
{
    /// <summary>
    /// Gets or sets the optional inclusive start date.
    /// </summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the optional inclusive end date.
    /// </summary>
    public DateOnly? EndDate { get; set; }
}
