using BookKeeping2.Services.Transactions;
using BookKeeping2.ViewModels.Transactions;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookKeeping2.Pages.Transactions;

/// <summary>
/// Displays non-deleted transactions.
/// </summary>
public sealed class IndexModel : PageModel
{
    private readonly ITransactionService transactionService;

    /// <summary>
    /// Initializes a new transactions index page model.
    /// </summary>
    /// <param name="transactionService">The transaction service.</param>
    public IndexModel(ITransactionService transactionService)
    {
        this.transactionService = transactionService;
    }

    /// <summary>
    /// Gets the visible transaction rows.
    /// </summary>
    public IReadOnlyList<TransactionListItemViewModel> Transactions { get; private set; } = [];

    /// <summary>
    /// Handles the index request.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnGetAsync()
    {
        Transactions = await transactionService.ListAsync();
    }
}
