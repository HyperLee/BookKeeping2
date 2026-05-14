using System.Globalization;
using BookKeeping2.Models.Common;
using BookKeeping2.Services.Budgets;
using BookKeeping2.Services.Time;
using BookKeeping2.ViewModels.Budgets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace BookKeeping2.Pages.Budgets;

/// <summary>
/// Handles monthly budget management.
/// </summary>
public sealed class IndexModel : PageModel
{
    private readonly IBudgetService budgetService;
    private readonly ITaipeiDateService dateService;
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a budget management page.
    /// </summary>
    /// <param name="budgetService">The budget service.</param>
    /// <param name="dateService">The Taipei date service.</param>
    /// <param name="localizer">The shared UI localizer.</param>
    public IndexModel(IBudgetService budgetService, ITaipeiDateService dateService, IStringLocalizer<SharedResource> localizer)
    {
        this.budgetService = budgetService;
        this.dateService = dateService;
        this.localizer = localizer;
    }

    /// <summary>
    /// Gets or sets the selected month query value in yyyy-MM format.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? Month { get; set; }

    /// <summary>
    /// Gets or sets the budget input.
    /// </summary>
    [BindProperty]
    public BudgetInputModel Input { get; set; } = new();

    /// <summary>
    /// Gets budget status rows.
    /// </summary>
    public IReadOnlyList<BudgetStatusViewModel> Budgets { get; private set; } = [];

    /// <summary>
    /// Gets form options.
    /// </summary>
    public BudgetFormOptionsViewModel Options { get; private set; } = new();

    /// <summary>
    /// Displays monthly budgets.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnGetAsync()
    {
        DateOnly selectedMonth = ResolveSelectedMonth();
        await LoadAsync(selectedMonth, resetInput: true);
    }

    /// <summary>
    /// Creates or updates a monthly budget.
    /// </summary>
    /// <returns>The post result.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        Input.BudgetMonth = new DateOnly(Input.BudgetMonth.Year, Input.BudgetMonth.Month, 1);
        if (!ModelState.IsValid)
        {
            await LoadAsync(Input.BudgetMonth, resetInput: false);
            return Page();
        }

        BudgetResult result = await budgetService.SaveAsync(Input);
        if (!result.Succeeded)
        {
            foreach ((string field, string[] messages) in result.Errors)
            {
                foreach (string message in messages)
                {
                    ModelState.AddModelError($"Input.{field}", message);
                }
            }

            await LoadAsync(Input.BudgetMonth, resetInput: false);
            return Page();
        }

        TempData["StatusMessage"] = localizer["預算已儲存。"].Value;
        return RedirectToPage(new { Month = Input.BudgetMonth.ToString("yyyy-MM", CultureInfo.InvariantCulture) });
    }

    /// <summary>
    /// Deletes a monthly budget.
    /// </summary>
    /// <param name="id">The budget identifier.</param>
    /// <returns>The post result.</returns>
    public async Task<IActionResult> OnPostDeleteAsync(long id)
    {
        DateOnly selectedMonth = ResolveSelectedMonth();
        BudgetResult result = await budgetService.DeleteAsync(id);
        TempData["StatusMessage"] = result.Succeeded ? localizer["預算已刪除。"].Value : "找不到預算設定。";
        return RedirectToPage(new { Month = selectedMonth.ToString("yyyy-MM", CultureInfo.InvariantCulture) });
    }

    private async Task LoadAsync(DateOnly selectedMonth, bool resetInput)
    {
        selectedMonth = new DateOnly(selectedMonth.Year, selectedMonth.Month, 1);
        Month = selectedMonth.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        Budgets = await budgetService.ListMonthlyAsync(selectedMonth);
        Options = await budgetService.GetFormOptionsAsync();
        if (resetInput)
        {
            Input = new BudgetInputModel
            {
                BudgetMonth = selectedMonth,
                Currency = SupportedCurrency.LegacyDefaultCode
            };
        }
    }

    private DateOnly ResolveSelectedMonth()
    {
        if (string.IsNullOrWhiteSpace(Month))
        {
            DateOnly today = dateService.Today;
            return new DateOnly(today.Year, today.Month, 1);
        }

        if (DateOnly.TryParseExact(
            $"{Month}-01",
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateOnly selectedMonth))
        {
            return new DateOnly(selectedMonth.Year, selectedMonth.Month, 1);
        }

        ModelState.AddModelError(nameof(Month), "月份格式無效。");
        DateOnly fallback = dateService.Today;
        return new DateOnly(fallback.Year, fallback.Month, 1);
    }
}
