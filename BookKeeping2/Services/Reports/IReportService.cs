using BookKeeping2.ViewModels.Reports;

namespace BookKeeping2.Services.Reports;

/// <summary>
/// Provides bookkeeping report calculations.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Gets the monthly income, expense, category share, and trend report.
    /// </summary>
    /// <param name="year">The local Asia/Taipei year.</param>
    /// <param name="month">The local Asia/Taipei month.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The calculated monthly report.</returns>
    Task<MonthlyReportViewModel> GetMonthlyReportAsync(int year, int month, CancellationToken cancellationToken = default);
}
