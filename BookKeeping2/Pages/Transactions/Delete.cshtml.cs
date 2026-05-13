using BookKeeping2.Services.Transactions;
using BookKeeping2.ViewModels.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace BookKeeping2.Pages.Transactions;

/// <summary>
/// Handles transaction deletion confirmation.
/// </summary>
public sealed class DeleteModel : PageModel
{
    private readonly ITransactionService transactionService;
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a new delete page model.
    /// </summary>
    /// <param name="transactionService">The transaction service.</param>
    /// <param name="localizer">The shared UI localizer.</param>
    public DeleteModel(ITransactionService transactionService, IStringLocalizer<SharedResource> localizer)
    {
        this.transactionService = transactionService;
        this.localizer = localizer;
    }

    /// <summary>
    /// Gets the transaction selected for deletion.
    /// </summary>
    public TransactionListItemViewModel Transaction { get; private set; } = new();

    /// <summary>
    /// Displays the delete confirmation.
    /// </summary>
    /// <param name="id">The transaction identifier.</param>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnGetAsync(long id)
    {
        TransactionListItemViewModel? transaction = await transactionService.GetDetailsAsync(id);
        if (transaction is null)
        {
            return NotFound();
        }

        Transaction = transaction;
        return Page();
    }

    /// <summary>
    /// Soft deletes the transaction.
    /// </summary>
    /// <param name="id">The transaction identifier.</param>
    /// <returns>The post result.</returns>
    public async Task<IActionResult> OnPostAsync(long id)
    {
        TransactionResult result = await transactionService.SoftDeleteAsync(id);
        if (!result.Succeeded)
        {
            return NotFound();
        }

        TempData["StatusMessage"] = localizer["交易已刪除。"].Value;
        return RedirectToPage("/Transactions/Index");
    }
}
