using BookKeeping2.Models.Common;
using BookKeeping2.Services.AccountTransfers;
using BookKeeping2.Services.Time;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BookKeeping2.Pages.Transfers;

/// <summary>
/// Handles account transfer creation.
/// </summary>
public sealed class CreateModel : TransferFormPageModel
{
    private readonly IAccountTransferService transferService;
    private readonly ITaipeiDateService dateService;
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a new transfer create page model.
    /// </summary>
    /// <param name="transferService">The transfer service.</param>
    /// <param name="dateService">The Taipei date service.</param>
    /// <param name="localizer">The shared UI localizer.</param>
    public CreateModel(
        IAccountTransferService transferService,
        ITaipeiDateService dateService,
        IStringLocalizer<SharedResource> localizer)
    {
        this.transferService = transferService;
        this.dateService = dateService;
        this.localizer = localizer;
    }

    /// <summary>
    /// Displays the create form.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnGetAsync()
    {
        Input.TransferDate = dateService.Today;
        Input.Currency = SupportedCurrency.LegacyDefaultCode;
        Input.SubmissionToken = Guid.NewGuid().ToString("N");
        Options = await transferService.GetFormOptionsAsync(Input.Currency);
    }

    /// <summary>
    /// Creates the transfer when input is valid.
    /// </summary>
    /// <returns>The post result.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        Options = await transferService.GetFormOptionsAsync(Input.Currency);
        if (!ModelState.IsValid)
        {
            return Page();
        }

        AccountTransferResult result = await transferService.CreateAsync(Input);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return Page();
        }

        TempData["StatusMessage"] = localizer["轉帳已新增。"].Value;
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
