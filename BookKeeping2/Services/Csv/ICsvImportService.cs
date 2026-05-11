namespace BookKeeping2.Services.Csv;

/// <summary>
/// Imports transactions from the fixed CSV contract.
/// </summary>
public interface ICsvImportService
{
    /// <summary>
    /// Imports a CSV file, creating valid transactions and reporting row-level errors.
    /// </summary>
    /// <param name="command">The import command.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The import result.</returns>
    Task<CsvImportResult> ImportAsync(CsvImportCommand command, CancellationToken cancellationToken = default);
}
