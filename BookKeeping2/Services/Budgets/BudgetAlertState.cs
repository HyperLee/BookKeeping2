namespace BookKeeping2.Services.Budgets;

/// <summary>
/// Represents the warning level for a budget.
/// </summary>
public enum BudgetAlertState
{
    /// <summary>
    /// Usage is below 80 percent.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Usage is at least 80 percent and at most 100 percent.
    /// </summary>
    NearLimit = 1,

    /// <summary>
    /// Usage is greater than 100 percent.
    /// </summary>
    Exceeded = 2
}
