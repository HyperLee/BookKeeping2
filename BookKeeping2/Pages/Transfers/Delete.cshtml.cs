using BookKeeping2.Services.AccountTransfers;
using BookKeeping2.ViewModels.AccountTransfers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace BookKeeping2.Pages.Transfers;

/// <summary>
/// Handles account transfer deletion confirmation.
/// </summary>
public sealed class DeleteModel : PageModel
{
    private readonly IAccountTransferService transferService;
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a new transfer delete page model.
    /// </summary>
    /// <param name="transferService">The transfer service.</param>
    /// <param name="localizer">The shared UI localizer.</param>
    public DeleteModel(IAccountTransferService transferService, IStringLocalizer<SharedResource> localizer)
    {
        this.transferService = transferService;
        this.localizer = localizer;
    }

    /// <summary>
    /// Gets the transfer selected for deletion.
    /// </summary>
    public AccountTransferDetailsViewModel Transfer { get; private set; } = new();

    /// <summary>
    /// Displays the delete confirmation.
    /// </summary>
    /// <param name="id">The transfer identifier.</param>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnGetAsync(long id)
    {
        AccountTransferDetailsViewModel? transfer = await transferService.GetDetailsAsync(id);
        if (transfer is null)
        {
            return NotFound();
        }

        Transfer = transfer;
        return Page();
    }

    /// <summary>
    /// Soft deletes the transfer.
    /// </summary>
    /// <param name="id">The transfer identifier.</param>
    /// <returns>The post result.</returns>
    public async Task<IActionResult> OnPostAsync(long id)
    {
        AccountTransferResult result = await transferService.SoftDeleteAsync(id);
        if (!result.Succeeded)
        {
            return NotFound();
        }

        TempData["StatusMessage"] = localizer["轉帳已刪除。"].Value;
        return RedirectToPage("/Transactions/Index");
    }
}
