using BookKeeping2.Services.Reports;

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
    /// Gets or sets the monthly income total.
    /// </summary>
    public decimal TotalIncome { get; set; }

    /// <summary>
    /// Gets or sets the monthly expense total.
    /// </summary>
    public decimal TotalExpense { get; set; }

    /// <summary>
    /// Gets the monthly balance.
    /// </summary>
    public decimal Balance => TotalIncome - TotalExpense;

    /// <summary>
    /// Gets whether the report contains any income or expense data.
    /// </summary>
    public bool HasData => TotalIncome != 0m || TotalExpense != 0m;

    /// <summary>
    /// Gets or sets expense category share rows.
    /// </summary>
    public IReadOnlyList<CategoryShareViewModel> CategoryShares { get; set; } = [];

    /// <summary>
    /// Gets or sets daily trend points for the chart.
    /// </summary>
    public IReadOnlyList<ReportChartPoint> TrendPoints { get; set; } = [];
}

/// <summary>
/// Represents an expense category's share in the monthly report.
/// </summary>
public sealed class CategoryShareViewModel
{
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
