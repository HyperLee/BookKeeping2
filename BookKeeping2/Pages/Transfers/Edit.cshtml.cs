using BookKeeping2.Services.AccountTransfers;
using BookKeeping2.ViewModels.AccountTransfers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BookKeeping2.Pages.Transfers;

/// <summary>
/// Handles account transfer editing.
/// </summary>
public sealed class EditModel : TransferFormPageModel
{
    private readonly IAccountTransferService transferService;
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a new transfer edit page model.
    /// </summary>
    /// <param name="transferService">The transfer service.</param>
    /// <param name="localizer">The shared UI localizer.</param>
    public EditModel(IAccountTransferService transferService, IStringLocalizer<SharedResource> localizer)
    {
        this.transferService = transferService;
        this.localizer = localizer;
    }

    /// <summary>
    /// Displays the edit form.
    /// </summary>
    /// <param name="id">The transfer identifier.</param>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnGetAsync(long id)
    {
        AccountTransferInputModel? input = await transferService.GetForEditAsync(id);
        if (input is null)
        {
            return NotFound();
        }

        Input = input;
        Options = await transferService.GetFormOptionsAsync(Input.Currency);
        return Page();
    }

    /// <summary>
    /// Updates the transfer when input is valid.
    /// </summary>
    /// <param name="id">The transfer identifier.</param>
    /// <returns>The post result.</returns>
    public async Task<IActionResult> OnPostAsync(long id)
    {
        Options = await transferService.GetFormOptionsAsync(Input.Currency);
        if (!ModelState.IsValid)
        {
            return Page();
        }

        AccountTransferResult result = await transferService.UpdateAsync(id, Input);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return Page();
        }

        TempData["StatusMessage"] = localizer["轉帳已更新。"].Value;
        return RedirectToPage("/Transactions/Index");
    }

    private void AddErrors(AccountTransferResult result)
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
