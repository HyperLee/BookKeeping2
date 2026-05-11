using BookKeeping2.Data;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Services.Common;
using BookKeeping2.ViewModels.Reports;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping2.Services.Reports;

/// <summary>
/// EF Core implementation of monthly report calculations.
/// </summary>
public sealed class ReportService : IReportService
{
    private readonly AppDbContext dbContext;

    /// <summary>
    /// Initializes a new report service.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    public ReportService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<MonthlyReportViewModel> GetMonthlyReportAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        DateOnly monthStart = new(year, month, 1);
        DateOnly monthEnd = monthStart.AddMonths(1).AddDays(-1);

        List<Transaction> transactions = await dbContext.Transactions
            .AsNoTracking()
            .Include(transaction => transaction.Category)
            .Where(transaction => !transaction.IsDeleted
                && transaction.TransactionDate >= monthStart
                && transaction.TransactionDate <= monthEnd)
            .ToListAsync(cancellationToken);

        long incomeMinorUnits = transactions
            .Where(transaction => transaction.Type == TransactionType.Income)
            .Sum(transaction => transaction.AmountMinorUnits);
        long expenseMinorUnits = transactions
            .Where(transaction => transaction.Type == TransactionType.Expense)
            .Sum(transaction => transaction.AmountMinorUnits);

        IReadOnlyList<CategoryShareViewModel> categoryShares = transactions
            .Where(transaction => transaction.Type == TransactionType.Expense)
            .GroupBy(transaction => transaction.Category.Name)
            .Select(group =>
            {
                long amountMinorUnits = group.Sum(transaction => transaction.AmountMinorUnits);
                decimal amount = MoneyMinorUnitConverter.FromMinorUnits(amountMinorUnits);
                decimal percentage = expenseMinorUnits == 0
                    ? 0m
                    : Math.Round(amountMinorUnits * 100m / expenseMinorUnits, 2, MidpointRounding.AwayFromZero);

                return new CategoryShareViewModel
                {
                    CategoryName = group.Key,
                    Amount = amount,
                    Percentage = percentage
                };
            })
            .OrderByDescending(share => share.Amount)
            .ThenBy(share => share.CategoryName)
            .ToList();

        IReadOnlyList<ReportChartPoint> trendPoints = transactions
            .GroupBy(transaction => transaction.TransactionDate)
            .OrderBy(group => group.Key)
            .Select(group => new ReportChartPoint
            {
                Label = group.Key.ToString("MM-dd"),
                Income = MoneyMinorUnitConverter.FromMinorUnits(group
                    .Where(transaction => transaction.Type == TransactionType.Income)
                    .Sum(transaction => transaction.AmountMinorUnits)),
                Expense = MoneyMinorUnitConverter.FromMinorUnits(group
                    .Where(transaction => transaction.Type == TransactionType.Expense)
                    .Sum(transaction => transaction.AmountMinorUnits))
            })
            .ToList();

        return new MonthlyReportViewModel
        {
            Month = monthStart,
            TotalIncome = MoneyMinorUnitConverter.FromMinorUnits(incomeMinorUnits),
            TotalExpense = MoneyMinorUnitConverter.FromMinorUnits(expenseMinorUnits),
            CategoryShares = categoryShares,
            TrendPoints = trendPoints
        };
    }
}
