namespace BookKeeping2.Services.Reports;

/// <summary>
/// Represents a daily income and expense point for report charts.
/// </summary>
public sealed class ReportChartPoint
{
    /// <summary>
    /// Gets or sets the display label.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the income amount.
    /// </summary>
    public decimal Income { get; set; }

    /// <summary>
    /// Gets or sets the expense amount.
    /// </summary>
    public decimal Expense { get; set; }
}
