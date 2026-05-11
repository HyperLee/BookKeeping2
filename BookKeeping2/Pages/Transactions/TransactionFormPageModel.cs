using BookKeeping2.ViewModels.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookKeeping2.Pages.Transactions;

/// <summary>
/// Base page model for transaction forms.
/// </summary>
public abstract class TransactionFormPageModel : PageModel
{
    /// <summary>
    /// Gets or sets the transaction input.
    /// </summary>
    [BindProperty]
    public TransactionInputModel Input { get; set; } = new();

    /// <summary>
    /// Gets or sets form options.
    /// </summary>
    public TransactionFormOptionsViewModel Options { get; protected set; } = new();
}
