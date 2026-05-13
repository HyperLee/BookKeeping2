using BookKeeping2.Services.Transactions;
using BookKeeping2.ViewModels.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BookKeeping2.Pages.Transactions;

/// <summary>
/// Handles transaction editing.
/// </summary>
public sealed class EditModel : TransactionFormPageModel
{
    private readonly ITransactionService transactionService;
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a new edit page model.
    /// </summary>
    /// <param name="transactionService">The transaction service.</param>
    /// <param name="localizer">The shared UI localizer.</param>
    public EditModel(ITransactionService transactionService, IStringLocalizer<SharedResource> localizer)
    {
        this.transactionService = transactionService;
        this.localizer = localizer;
    }

    /// <summary>
    /// Displays the edit form.
    /// </summary>
    /// <param name="id">The transaction identifier.</param>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnGetAsync(long id)
    {
        TransactionInputModel? input = await transactionService.GetForEditAsync(id);
        if (input is null)
        {
            return NotFound();
        }

        Input = input;
        Options = await transactionService.GetFormOptionsAsync(Input.Type);
        return Page();
    }

    /// <summary>
    /// Updates the transaction when input is valid.
    /// </summary>
    /// <param name="id">The transaction identifier.</param>
    /// <returns>The post result.</returns>
    public async Task<IActionResult> OnPostAsync(long id)
    {
        Options = await transactionService.GetFormOptionsAsync(Input.Type);
        if (!ModelState.IsValid)
        {
            return Page();
        }

        TransactionResult result = await transactionService.UpdateAsync(id, Input);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return Page();
        }

        TempData["StatusMessage"] = localizer["交易已更新。"].Value;
        return RedirectToPage("/Transactions/Index");
    }

    private void AddErrors(TransactionResult result)
    {
        foreach ((string field, string[] messages) in result.Errors)
        {
            foreach (string message in messages)
            {
                ModelState.AddModelError(string.IsNullOrEmpty(field) ? string.Empty : $"Input.{field}", message);
            }
        }
    }
}
