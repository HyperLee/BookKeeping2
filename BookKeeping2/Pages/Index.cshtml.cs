using BookKeeping2.Data;
using BookKeeping2.Models.Common;
using BookKeeping2.Services.Common;
using BookKeeping2.Services.Time;
using BookKeeping2.ViewModels.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookKeeping2.Pages;

/// <summary>
/// Displays the bookkeeping dashboard.
/// </summary>
public class IndexModel : PageModel
{
    private readonly AppDbContext dbContext;
    private readonly ITaipeiDateService dateService;

    /// <summary>
    /// Initializes a new dashboard page model.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    /// <param name="dateService">The Taipei date service.</param>
    public IndexModel(AppDbContext dbContext, ITaipeiDateService dateService)
    {
        this.dbContext = dbContext;
        this.dateService = dateService;
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
    }
}
