using BookKeeping2.ViewModels.Budgets;

namespace BookKeeping2.Services.Budgets;

/// <summary>
/// Provides monthly budget management and calculations.
/// </summary>
public interface IBudgetService
{
    /// <summary>
    /// Lists budget statuses for a month.
    /// </summary>
    /// <param name="budgetMonth">Any date within the requested month.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The calculated budget statuses.</returns>
    Task<IReadOnlyList<BudgetStatusViewModel>> ListMonthlyAsync(DateOnly budgetMonth, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets form options for budget management.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The budget form options.</returns>
    Task<BudgetFormOptionsViewModel> GetFormOptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a category budget for a month.
    /// </summary>
    /// <param name="input">The budget input.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The write result.</returns>
    Task<BudgetResult> SaveAsync(BudgetInputModel input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a monthly budget.
    /// </summary>
    /// <param name="id">The budget identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The delete result.</returns>
    Task<BudgetResult> DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records an audit event if the related category budget is near or over the limit.
    /// </summary>
    /// <param name="categoryId">The expense category identifier.</param>
    /// <param name="transactionDate">The transaction date that may affect the budget month.</param>
    /// <param name="currency">The transaction currency code.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AuditWarningForCategoryMonthAsync(long categoryId, DateOnly transactionDate, string? currency = null, CancellationToken cancellationToken = default);
}
