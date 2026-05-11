using BookKeeping2.Services.Transactions;
using BookKeeping2.ViewModels.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookKeeping2.Pages.Transactions;

/// <summary>
/// Displays non-deleted transactions.
/// </summary>
public sealed class IndexModel : PageModel
{
    private readonly ITransactionQueryService transactionQueryService;
    private readonly TransactionFormOptionsService formOptionsService;

    /// <summary>
    /// Initializes a new transactions index page model.
    /// </summary>
    /// <param name="transactionQueryService">The transaction query service.</param>
    /// <param name="formOptionsService">The form options service.</param>
    public IndexModel(ITransactionQueryService transactionQueryService, TransactionFormOptionsService formOptionsService)
    {
        this.transactionQueryService = transactionQueryService;
        this.formOptionsService = formOptionsService;
    }

    /// <summary>
    /// Gets or sets the current filter.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public TransactionFilterInputModel? Filter { get; set; } = new();

    /// <summary>
    /// Gets the paged transaction rows.
    /// </summary>
    public PagedTransactionListViewModel Transactions { get; private set; } = new();

    /// <summary>
    /// Gets filter options.
    /// </summary>
    public TransactionFormOptionsViewModel Options { get; private set; } = new();

    /// <summary>
    /// Handles the index request.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnGetAsync()
    {
        Filter ??= new TransactionFilterInputModel();
        NormalizeFilter();
        if (!Request.Query.Keys.Any(key => key.StartsWith("Filter.", StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.Clear();
        }

        ModelState.Remove(nameof(Filter));
        ModelState.Remove("Filter.Page");
        ModelState.Remove("Filter.PageSize");
        ValidateFilter();
        Options = await formOptionsService.GetOptionsAsync();
        if (!ModelState.IsValid)
        {
            return;
        }

        Transactions = await transactionQueryService.SearchAsync(new TransactionQuery
        {
            StartDate = Filter.StartDate,
            EndDate = Filter.EndDate,
            CategoryId = Filter.CategoryId,
            AccountId = Filter.AccountId,
            MinAmount = Filter.MinAmount,
            MaxAmount = Filter.MaxAmount,
            Keyword = Filter.Keyword,
            Page = Filter.Page,
            PageSize = Filter.PageSize
        });
    }

    private void ValidateFilter()
    {
        Filter ??= new TransactionFilterInputModel();
        if (Filter.StartDate.HasValue && Filter.EndDate.HasValue && Filter.EndDate.Value < Filter.StartDate.Value)
        {
            ModelState.AddModelError("Filter.EndDate", "結束日期不可早於起始日期。");
        }

        if (Filter.MinAmount.HasValue && Filter.MaxAmount.HasValue && Filter.MaxAmount.Value < Filter.MinAmount.Value)
        {
            ModelState.AddModelError("Filter.MaxAmount", "最大金額不可小於最小金額。");
        }
    }

    private void NormalizeFilter()
    {
        if (Filter is null)
        {
            return;
        }

        if (Filter.CategoryId <= 0)
        {
            Filter.CategoryId = null;
        }

        if (Filter.AccountId <= 0)
        {
            Filter.AccountId = null;
        }

        if (Filter.Page < 1)
        {
            Filter.Page = 1;
        }

        if (Filter.PageSize < 1)
        {
            Filter.PageSize = 50;
        }
    }
}
