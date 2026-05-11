namespace BookKeeping2.Services.Csv;

/// <summary>
/// Exports transactions to CSV.
/// </summary>
public interface ICsvExportService
{
    /// <summary>
    /// Exports non-deleted transactions matching the supplied options.
    /// </summary>
    /// <param name="options">The export options.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The generated CSV content.</returns>
    Task<CsvExportResult> ExportAsync(CsvExportOptions options, CancellationToken cancellationToken = default);
}
