using System.ComponentModel.DataAnnotations;
using BookKeeping2.Services.Budgets;

namespace BookKeeping2.ViewModels.Budgets;

/// <summary>
/// Input model for monthly budget management.
/// </summary>
public sealed class BudgetInputModel
{
    /// <summary>
    /// Gets or sets the expense category identifier.
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "請選擇支出分類。")]
    [Display(Name = "支出分類")]
    public long CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the first day of the budget month.
    /// </summary>
    [Display(Name = "預算月份")]
    public DateOnly BudgetMonth { get; set; }

    /// <summary>
    /// Gets or sets the budget amount.
    /// </summary>
    [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = "預算金額必須大於 0 且不可超過 TWD 999,999,999.99。")]
    [Display(Name = "預算金額")]
    public decimal Amount { get; set; }
}

/// <summary>
/// Represents a monthly budget status row.
/// </summary>
public sealed class BudgetStatusViewModel
{
    /// <summary>
    /// Gets or sets the budget identifier.
    /// </summary>
    public long BudgetId { get; set; }

    /// <summary>
    /// Gets or sets the category identifier.
    /// </summary>
    public long CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first day of the budget month.
    /// </summary>
    public DateOnly BudgetMonth { get; set; }

    /// <summary>
    /// Gets or sets the budget amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the spent amount in the same month and category.
    /// </summary>
    public decimal SpentAmount { get; set; }

    /// <summary>
    /// Gets or sets the usage rate as a ratio.
    /// </summary>
    public decimal UsageRate { get; set; }

    /// <summary>
    /// Gets the usage rate as a percentage.
    /// </summary>
    public decimal UsagePercentage => UsageRate * 100m;

    /// <summary>
    /// Gets or sets the remaining amount when the budget has not been exceeded.
    /// </summary>
    public decimal RemainingAmount { get; set; }

    /// <summary>
    /// Gets or sets the overspent amount when the budget has been exceeded.
    /// </summary>
    public decimal OverspentAmount { get; set; }

    /// <summary>
    /// Gets or sets the alert state.
    /// </summary>
    public BudgetAlertState AlertState { get; set; }

    /// <summary>
    /// Gets the Traditional Chinese alert text.
    /// </summary>
    public string AlertText => AlertState switch
    {
        BudgetAlertState.Exceeded => $"已超出預算 {OverspentAmount:N0}",
        BudgetAlertState.NearLimit => "接近預算上限",
        _ => "使用率正常"
    };

    /// <summary>
    /// Gets the progress bar CSS class.
    /// </summary>
    public string ProgressBarClass => AlertState switch
    {
        BudgetAlertState.Exceeded => "bg-danger",
        BudgetAlertState.NearLimit => "bg-warning text-dark",
        _ => "bg-success"
    };
}

/// <summary>
/// Represents an expense category option for budget forms.
/// </summary>
public sealed class BudgetCategoryOptionViewModel
{
    /// <summary>
    /// Gets or sets the category identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Represents budget form options.
/// </summary>
public sealed class BudgetFormOptionsViewModel
{
    /// <summary>
    /// Gets or sets the selectable expense categories.
    /// </summary>
    public IReadOnlyList<BudgetCategoryOptionViewModel> Categories { get; set; } = [];
}
