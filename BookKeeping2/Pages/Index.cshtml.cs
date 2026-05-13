using BookKeeping2.Data;
using BookKeeping2.Localization;
using BookKeeping2.Models.Common;
using BookKeeping2.Services.Accounts;
using BookKeeping2.Services.Budgets;
using BookKeeping2.Services.Common;
using BookKeeping2.Services.Time;
using BookKeeping2.ViewModels.Budgets;
using BookKeeping2.ViewModels.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping2.Pages;

/// <summary>
/// Displays the bookkeeping dashboard.
/// </summary>
public class IndexModel : PageModel
{
    private readonly AppDbContext dbContext;
    private readonly ITaipeiDateService dateService;
    private readonly IBudgetService budgetService;
    private readonly IAccountService accountService;

    /// <summary>
    /// Initializes a new dashboard page model.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    /// <param name="dateService">The Taipei date service.</param>
    /// <param name="budgetService">The budget service.</param>
    /// <param name="accountService">The account service.</param>
    public IndexModel(AppDbContext dbContext, ITaipeiDateService dateService, IBudgetService budgetService, IAccountService accountService)
    {
        this.dbContext = dbContext;
        this.dateService = dateService;
        this.budgetService = budgetService;
        this.accountService = accountService;
    }

    /// <summary>
    /// Gets the current month income total.
    /// </summary>
    public decimal MonthlyIncome { get; private set; }

    /// <summary>
    /// Gets the current month expense total.
    /// </summary>
    public decimal MonthlyExpense { get; private set; }

    /// <summary>
    /// Gets the current month balance.
    /// </summary>
    public decimal MonthlyBalance => MonthlyIncome - MonthlyExpense;

    /// <summary>
    /// Gets recent visible transactions.
    /// </summary>
    public IReadOnlyList<TransactionListItemViewModel> RecentTransactions { get; private set; } = [];

    /// <summary>
    /// Gets account balance summaries.
    /// </summary>
    public IReadOnlyList<AccountBalanceSummary> AccountBalances { get; private set; } = [];

    /// <summary>
    /// Gets current month budget summaries.
    /// </summary>
    public IReadOnlyList<BudgetStatusViewModel> BudgetSummaries { get; private set; } = [];

    /// <summary>
    /// Gets the resolved language used to mark the selected homepage language option.
    /// </summary>
    public string SelectedUiLanguage => UiLanguageOptions.NormalizeUiCulture(Request.Cookies[UiLanguageOptions.CookieName]);

    /// <summary>
    /// Handles the dashboard request.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnGetAsync()
    {
        DateOnly today = dateService.Today;
        DateOnly monthStart = new(today.Year, today.Month, 1);
        DateOnly monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var monthTransactions = await dbContext.Transactions
            .AsNoTracking()
            .Include(transaction => transaction.Category)
            .Include(transaction => transaction.Account)
            .Where(transaction => !transaction.IsDeleted
                && transaction.TransactionDate >= monthStart
                && transaction.TransactionDate <= monthEnd)
            .ToListAsync();

        MonthlyIncome = MoneyMinorUnitConverter.FromMinorUnits(monthTransactions
            .Where(transaction => transaction.Type == TransactionType.Income)
            .Sum(transaction => transaction.AmountMinorUnits));
        MonthlyExpense = MoneyMinorUnitConverter.FromMinorUnits(monthTransactions
            .Where(transaction => transaction.Type == TransactionType.Expense)
            .Sum(transaction => transaction.AmountMinorUnits));

        RecentTransactions = monthTransactions
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Id)
            .Take(5)
            .Select(transaction => new TransactionListItemViewModel
            {
                Id = transaction.Id,
                TransactionDate = transaction.TransactionDate,
                Type = transaction.Type,
                Amount = transaction.Amount,
                CategoryName = transaction.Category.Name,
                AccountName = transaction.Account.Name,
                Note = transaction.Note
            })
            .ToList();

        AccountBalances = await accountService.GetBalanceSummariesAsync(includeArchived: false);
        BudgetSummaries = await budgetService.ListMonthlyAsync(monthStart);
    }

    /// <summary>
    /// Updates the site interface language preference.
    /// </summary>
    /// <param name="uiLanguage">The selected UI language code.</param>
    /// <returns>A redirect back to the homepage.</returns>
    public IActionResult OnPostLanguage(string uiLanguage)
    {
        string selectedLanguage = UiLanguageOptions.NormalizeUiCulture(uiLanguage);
        Response.Cookies.Append(
            UiLanguageOptions.CookieName,
            selectedLanguage,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.Add(UiLanguageOptions.CookieLifetime),
                HttpOnly = true,
                Path = "/",
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps,
                IsEssential = true
            });

        return RedirectToPage();
    }
}
