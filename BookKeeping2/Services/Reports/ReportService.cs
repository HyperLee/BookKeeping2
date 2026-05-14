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

        Dictionary<string, int> currencyOrder = SupportedCurrency.Options.ToDictionary(option => option.Code, option => option.SortOrder);
        IReadOnlyList<MonthlyCurrencyReportViewModel> currencyBuckets = transactions
            .GroupBy(transaction => transaction.Currency)
            .OrderBy(group => currencyOrder.GetValueOrDefault(group.Key, int.MaxValue))
            .ThenBy(group => group.Key)
            .Select(group =>
            {
                string currency = group.Key;
                long incomeMinorUnits = group
                    .Where(transaction => transaction.Type == TransactionType.Income)
                    .Sum(transaction => transaction.AmountMinorUnits);
                long expenseMinorUnits = group
                    .Where(transaction => transaction.Type == TransactionType.Expense)
                    .Sum(transaction => transaction.AmountMinorUnits);

                IReadOnlyList<CategoryShareViewModel> categoryShares = group
                    .Where(transaction => transaction.Type == TransactionType.Expense)
                    .GroupBy(transaction => transaction.Category.Name)
                    .Select(categoryGroup =>
                    {
                        long amountMinorUnits = categoryGroup.Sum(transaction => transaction.AmountMinorUnits);
                        decimal amount = MoneyMinorUnitConverter.FromMinorUnits(amountMinorUnits);
                        decimal percentage = expenseMinorUnits == 0
                            ? 0m
                            : Math.Round(amountMinorUnits * 100m / expenseMinorUnits, 2, MidpointRounding.AwayFromZero);

                        return new CategoryShareViewModel
                        {
                            Currency = currency,
                            CategoryName = categoryGroup.Key,
                            Amount = amount,
                            Percentage = percentage
                        };
                    })
                    .OrderByDescending(share => share.Amount)
                    .ThenBy(share => share.CategoryName)
                    .ToList();

                IReadOnlyList<ReportChartPoint> trendPoints = group
                    .GroupBy(transaction => transaction.TransactionDate)
                    .OrderBy(dateGroup => dateGroup.Key)
                    .Select(dateGroup => new ReportChartPoint
                    {
                        Currency = currency,
                        Label = dateGroup.Key.ToString("MM-dd"),
                        Income = MoneyMinorUnitConverter.FromMinorUnits(dateGroup
                            .Where(transaction => transaction.Type == TransactionType.Income)
                            .Sum(transaction => transaction.AmountMinorUnits)),
                        Expense = MoneyMinorUnitConverter.FromMinorUnits(dateGroup
                            .Where(transaction => transaction.Type == TransactionType.Expense)
                            .Sum(transaction => transaction.AmountMinorUnits))
                    })
                    .ToList();

                return new MonthlyCurrencyReportViewModel
                {
                    Month = monthStart,
                    Currency = currency,
                    TotalIncome = MoneyMinorUnitConverter.FromMinorUnits(incomeMinorUnits),
                    TotalExpense = MoneyMinorUnitConverter.FromMinorUnits(expenseMinorUnits),
                    CategoryShares = categoryShares,
                    TrendPoints = trendPoints
                };
            })
            .ToList();

        return new MonthlyReportViewModel
        {
            Month = monthStart,
            CurrencyBuckets = currencyBuckets
        };
    }
}
