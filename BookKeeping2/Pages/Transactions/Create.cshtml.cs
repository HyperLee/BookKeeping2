using BookKeeping2.Models.Common;
using BookKeeping2.Services.Time;
using BookKeeping2.Services.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BookKeeping2.Pages.Transactions;

/// <summary>
/// Handles transaction creation.
/// </summary>
public sealed class CreateModel : TransactionFormPageModel
{
    private readonly ITransactionService transactionService;
    private readonly ITaipeiDateService dateService;
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a new create page model.
    /// </summary>
    /// <param name="transactionService">The transaction service.</param>
    /// <param name="dateService">The Taipei date service.</param>
    /// <param name="localizer">The shared UI localizer.</param>
    public CreateModel(ITransactionService transactionService, ITaipeiDateService dateService, IStringLocalizer<SharedResource> localizer)
    {
        this.transactionService = transactionService;
        this.dateService = dateService;
        this.localizer = localizer;
    }

    /// <summary>
    /// Displays the create form.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnGetAsync()
    {
        Input.TransactionDate = dateService.Today;
        Input.Type = TransactionType.Expense;
        Input.Currency = SupportedCurrency.LegacyDefaultCode;
        Options = await transactionService.GetFormOptionsAsync(Input.Type, Input.Currency);
    }

    /// <summary>
    /// Creates the transaction when input is valid.
    /// </summary>
    /// <returns>The post result.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        Options = await transactionService.GetFormOptionsAsync(Input.Type, Input.Currency);
        if (!ModelState.IsValid)
        {
            return Page();
        }

        TransactionResult result = await transactionService.CreateAsync(Input);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return Page();
        }

        TempData["StatusMessage"] = localizer["交易已新增。"].Value;
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
