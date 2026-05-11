using BookKeeping2.Services.Accounts;
using BookKeeping2.ViewModels.Accounts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookKeeping2.Pages.Accounts;

/// <summary>
/// Handles account management.
/// </summary>
public sealed class IndexModel : PageModel
{
    private readonly IAccountService accountService;

    /// <summary>
    /// Initializes an account management page.
    /// </summary>
    /// <param name="accountService">The account service.</param>
    public IndexModel(IAccountService accountService)
    {
        this.accountService = accountService;
    }

    /// <summary>
    /// Gets or sets the account input.
    /// </summary>
    [BindProperty]
    public AccountInputModel Input { get; set; } = new();

    /// <summary>
    /// Gets account rows.
    /// </summary>
    public IReadOnlyList<AccountBalanceSummary> Accounts { get; private set; } = [];

    /// <summary>
    /// Displays accounts.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnGetAsync()
    {
        Accounts = await accountService.GetBalanceSummariesAsync();
    }

    /// <summary>
    /// Creates an account.
    /// </summary>
    /// <returns>The post result.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Accounts = await accountService.GetBalanceSummariesAsync();
            return Page();
        }

        AccountResult result = await accountService.CreateAsync(Input.Name, Input.Type, Input.OpeningBalance, Input.IconKey, Input.DisplayOrder);
        if (!result.Succeeded)
        {
            foreach ((string field, string[] messages) in result.Errors)
            {
                foreach (string message in messages)
                {
                    ModelState.AddModelError($"Input.{field}", message);
                }
            }

            Accounts = await accountService.GetBalanceSummariesAsync();
            return Page();
        }

        TempData["StatusMessage"] = "帳戶已新增。";
        return RedirectToPage();
    }
}
