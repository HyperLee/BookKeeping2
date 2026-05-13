using BookKeeping2.Data;
using BookKeeping2.Localization;
using BookKeeping2.Models.Common;
using BookKeeping2.ViewModels.Transactions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping2.Services.Transactions;

/// <summary>
/// Loads category and account options for transaction forms.
/// </summary>
public sealed class TransactionFormOptionsService
{
    private readonly AppDbContext dbContext;

    /// <summary>
    /// Initializes a transaction form options service.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    public TransactionFormOptionsService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <summary>
    /// Gets category and account options.
    /// </summary>
    /// <param name="type">The selected transaction type.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Form options.</returns>
    public async Task<TransactionFormOptionsViewModel> GetOptionsAsync(TransactionType? type = null, CancellationToken cancellationToken = default)
    {
        var categories = dbContext.Categories.AsNoTracking().Where(category => !category.IsArchived);
        if (type is not null)
        {
            categories = categories.Where(category => category.Type == type);
        }

        var categoryRows = await categories
            .OrderBy(category => category.Type)
            .ThenBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .Select(category => new
            {
                category.Id,
                category.Name,
                category.IsDefault
            })
            .ToListAsync(cancellationToken);

        return new TransactionFormOptionsViewModel
        {
            Categories = categoryRows
                .Select(category => new SelectListItem(SystemDisplayLocalizer.GetCategoryName(category.Name, category.IsDefault), category.Id.ToString()))
                .ToList(),
            Accounts = await dbContext.Accounts
                .AsNoTracking()
                .Where(account => !account.IsArchived)
                .OrderBy(account => account.DisplayOrder)
                .ThenBy(account => account.Name)
                .Select(account => new SelectListItem(account.Name, account.Id.ToString()))
                .ToListAsync(cancellationToken)
        };
    }
}
