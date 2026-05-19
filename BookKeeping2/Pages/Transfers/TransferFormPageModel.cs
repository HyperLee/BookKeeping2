using BookKeeping2.ViewModels.AccountTransfers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookKeeping2.Pages.Transfers;

/// <summary>
/// Base page model for account transfer forms.
/// </summary>
public abstract class TransferFormPageModel : PageModel
{
    /// <summary>
    /// Gets or sets the transfer input.
    /// </summary>
    [BindProperty]
    public AccountTransferInputModel Input { get; set; } = new();

    /// <summary>
    /// Gets or sets form options.
    /// </summary>
    public AccountTransferFormOptionsViewModel Options { get; protected set; } = new();
}
