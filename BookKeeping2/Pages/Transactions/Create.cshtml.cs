using BookKeeping2.Models.Common;
using BookKeeping2.Services.Time;
using BookKeeping2.Services.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace BookKeeping2.Pages.Transactions;

/// <summary>
/// Handles transaction creation.
/// </summary>
public sealed class CreateModel : TransactionFormPageModel
{
    private readonly ITransactionService transactionService;
    private readonly ITaipeiDateService dateService;

    /// <summary>
    /// Initializes a new create page model.
    /// </summary>
    /// <param name="transactionService">The transaction service.</param>
    /// <param name="dateService">The Taipei date service.</param>
    public CreateModel(ITransactionService transactionService, ITaipeiDateService dateService)
    {
        this.transactionService = transactionService;
        this.dateService = dateService;
    }

    /// <summary>
    /// Displays the create form.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnGetAsync()
    {
        Input.TransactionDate = dateService.Today;
        Input.Type = TransactionType.Expense;
        Options = await transactionService.GetFormOptionsAsync(Input.Type);
    }

    /// <summary>
    /// Creates the transaction when input is valid.
    /// </summary>
    /// <returns>The post result.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        Options = await transactionService.GetFormOptionsAsync(Input.Type);
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

        TempData["StatusMessage"] = "交易已新增。";
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
