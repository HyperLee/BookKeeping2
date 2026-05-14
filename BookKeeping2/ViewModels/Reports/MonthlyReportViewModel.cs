using BookKeeping2.Services.Reports;
using BookKeeping2.Models.Common;

namespace BookKeeping2.ViewModels.Reports;

/// <summary>
/// Represents the calculated monthly report shown on the reports page.
/// </summary>
public sealed class MonthlyReportViewModel
{
    /// <summary>
    /// Gets or sets the first day of the reported month.
    /// </summary>
    public DateOnly Month { get; set; }

    /// <summary>
    /// Gets or sets per-currency monthly report buckets.
    /// </summary>
    public IReadOnlyList<MonthlyCurrencyReportViewModel> CurrencyBuckets { get; set; } = [];

    /// <summary>
    /// Gets the single-currency income total when exactly one bucket exists.
    /// </summary>
    public decimal TotalIncome => CurrencyBuckets.Count == 1 ? CurrencyBuckets[0].TotalIncome : 0m;

    /// <summary>
    /// Gets the single-currency expense total when exactly one bucket exists.
    /// </summary>
    public decimal TotalExpense => CurrencyBuckets.Count == 1 ? CurrencyBuckets[0].TotalExpense : 0m;

    /// <summary>
    /// Gets the single-currency balance when exactly one bucket exists.
    /// </summary>
    public decimal Balance => CurrencyBuckets.Count == 1 ? CurrencyBuckets[0].Balance : 0m;

    /// <summary>
    /// Gets whether the report contains any income or expense data.
    /// </summary>
    public bool HasData => CurrencyBuckets.Count > 0;

    /// <summary>
    /// Gets all expense category share rows across currency buckets without summing them.
    /// </summary>
    public IReadOnlyList<CategoryShareViewModel> CategoryShares => CurrencyBuckets.SelectMany(bucket => bucket.CategoryShares).ToList();

    /// <summary>
    /// Gets all daily trend points across currency buckets without summing them.
    /// </summary>
    public IReadOnlyList<ReportChartPoint> TrendPoints => CurrencyBuckets.SelectMany(bucket => bucket.TrendPoints).ToList();
}

/// <summary>
/// Represents one same-currency bucket in a monthly report.
/// </summary>
public sealed class MonthlyCurrencyReportViewModel
{
    /// <summary>
    /// Gets or sets the first day of the reported month.
    /// </summary>
    public DateOnly Month { get; set; }

    /// <summary>
    /// Gets or sets the report currency code.
    /// </summary>
    public string Currency { get; set; } = SupportedCurrency.LegacyDefaultCode;

    /// <summary>
    /// Gets the report currency display name.
    /// </summary>
    public string CurrencyDisplayName => SupportedCurrency.GetDisplayName(Currency);

    /// <summary>
    /// Gets or sets the same-currency monthly income total.
    /// </summary>
    public decimal TotalIncome { get; set; }

    /// <summary>
    /// Gets or sets the same-currency monthly expense total.
    /// </summary>
    public decimal TotalExpense { get; set; }

    /// <summary>
    /// Gets the same-currency monthly balance.
    /// </summary>
    public decimal Balance => TotalIncome - TotalExpense;

    /// <summary>
    /// Gets or sets same-currency expense category share rows.
    /// </summary>
    public IReadOnlyList<CategoryShareViewModel> CategoryShares { get; set; } = [];

    /// <summary>
    /// Gets or sets same-currency daily trend points for the chart.
    /// </summary>
    public IReadOnlyList<ReportChartPoint> TrendPoints { get; set; } = [];
}

/// <summary>
/// Represents an expense category's share in the monthly report.
/// </summary>
public sealed class CategoryShareViewModel
{
    /// <summary>
    /// Gets or sets the category share currency code.
    /// </summary>
    public string Currency { get; set; } = SupportedCurrency.LegacyDefaultCode;

    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category expense amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the percentage of total monthly expenses.
    /// </summary>
    public decimal Percentage { get; set; }
}
