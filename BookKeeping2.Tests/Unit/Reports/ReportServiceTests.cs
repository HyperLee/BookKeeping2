using System.Diagnostics;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Services.Reports;
using BookKeeping2.Tests.TestSupport;
using BookKeeping2.ViewModels.Reports;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookKeeping2.Tests.Unit.Reports;

public sealed class ReportServiceTests
{
    [Fact]
    public async Task GetMonthlyReportAsync_calculates_totals_category_share_and_cross_year_month()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        (Account account, Category food, Category transport, Category salary) = await SeedAsync(context);
        context.Transactions.AddRange(
            Create(account.Id, food.Id, TransactionType.Expense, new DateOnly(2026, 1, 3), 100m),
            Create(account.Id, transport.Id, TransactionType.Expense, new DateOnly(2026, 1, 4), 300m),
            Create(account.Id, salary.Id, TransactionType.Income, new DateOnly(2026, 1, 5), 1_000m),
            Create(account.Id, food.Id, TransactionType.Expense, new DateOnly(2025, 12, 31), 999m));
        await context.SaveChangesAsync();
        var service = new ReportService(context);

        var report = await service.GetMonthlyReportAsync(2026, 1);

        MonthlyCurrencyReportViewModel bucket = Assert.Single(report.CurrencyBuckets);
        Assert.Equal(TestDataBuilder.TwdCurrency, bucket.Currency);
        Assert.Equal(1_000m, bucket.TotalIncome);
        Assert.Equal(400m, bucket.TotalExpense);
        Assert.Equal(600m, bucket.Balance);
        Assert.Equal(2, bucket.CategoryShares.Count);
        Assert.Contains(bucket.CategoryShares, share => share.CategoryName == "交通" && share.Amount == 300m && share.Percentage == 75m);
        Assert.DoesNotContain(bucket.CategoryShares, share => share.Amount == 999m);
    }

    [Fact]
    public async Task GetMonthlyReportAsync_groups_totals_category_shares_and_trends_by_currency()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        (Account twdAccount, Account usdAccount, Category food, _, Category salary) = await SeedMultiCurrencyAsync(context);
        context.Transactions.AddRange(
            Create(twdAccount.Id, food.Id, TransactionType.Expense, new DateOnly(2026, 5, 3), 100m, TestDataBuilder.TwdCurrency),
            Create(twdAccount.Id, salary.Id, TransactionType.Income, new DateOnly(2026, 5, 4), 1_000m, TestDataBuilder.TwdCurrency),
            Create(usdAccount.Id, food.Id, TransactionType.Expense, new DateOnly(2026, 5, 3), 100m, TestDataBuilder.UsdCurrency),
            Create(usdAccount.Id, salary.Id, TransactionType.Income, new DateOnly(2026, 5, 4), 500m, TestDataBuilder.UsdCurrency));
        await context.SaveChangesAsync();
        var service = new ReportService(context);

        MonthlyReportViewModel report = await service.GetMonthlyReportAsync(2026, 5);

        Assert.Equal(2, report.CurrencyBuckets.Count);
        MonthlyCurrencyReportViewModel twd = Assert.Single(report.CurrencyBuckets, bucket => bucket.Currency == TestDataBuilder.TwdCurrency);
        MonthlyCurrencyReportViewModel usd = Assert.Single(report.CurrencyBuckets, bucket => bucket.Currency == TestDataBuilder.UsdCurrency);
        Assert.Equal(1_000m, twd.TotalIncome);
        Assert.Equal(100m, twd.TotalExpense);
        Assert.Equal(900m, twd.Balance);
        Assert.Equal(500m, usd.TotalIncome);
        Assert.Equal(100m, usd.TotalExpense);
        Assert.Equal(400m, usd.Balance);
        Assert.All(twd.CategoryShares, share => Assert.Equal(TestDataBuilder.TwdCurrency, share.Currency));
        Assert.All(usd.CategoryShares, share => Assert.Equal(TestDataBuilder.UsdCurrency, share.Currency));
        Assert.All(twd.TrendPoints, point => Assert.Equal(TestDataBuilder.TwdCurrency, point.Currency));
        Assert.All(usd.TrendPoints, point => Assert.Equal(TestDataBuilder.UsdCurrency, point.Currency));
    }

    [Fact]
    public async Task GetMonthlyReportAsync_completes_100_transactions_under_two_seconds()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        (Account account, Category food, _, _) = await SeedAsync(context);
        for (int i = 0; i < 100; i++)
        {
            context.Transactions.Add(Create(account.Id, food.Id, TransactionType.Expense, new DateOnly(2026, 2, (i % 28) + 1), 10m));
        }
        await context.SaveChangesAsync();
        var service = new ReportService(context);
        Stopwatch stopwatch = Stopwatch.StartNew();

        var report = await service.GetMonthlyReportAsync(2026, 2);

        stopwatch.Stop();
        MonthlyCurrencyReportViewModel bucket = Assert.Single(report.CurrencyBuckets);
        Assert.Equal(1_000m, bucket.TotalExpense);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2));
    }

    private static Transaction Create(long accountId, long categoryId, TransactionType type, DateOnly date, decimal amount, string currency = TestDataBuilder.TwdCurrency)
    {
        return new Transaction
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Type = type,
            TransactionDate = date,
            Currency = currency,
            Amount = amount,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "測試"
        };
    }

    private static async Task<(Account Account, Category Food, Category Transport, Category Salary)> SeedAsync(AppDbContext context)
    {
        var account = new Account { Name = "現金", NormalizedName = "現金", Type = AccountType.Cash, IconKey = "wallet", Currency = "TWD", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var food = new Category { Name = "餐飲", NormalizedName = "餐飲", Type = TransactionType.Expense, IconKey = "tag", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var transport = new Category { Name = "交通", NormalizedName = "交通", Type = TransactionType.Expense, IconKey = "tag", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var salary = new Category { Name = "薪資", NormalizedName = "薪資", Type = TransactionType.Income, IconKey = "tag", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        context.AddRange(account, food, transport, salary);
        await context.SaveChangesAsync();
        return (account, food, transport, salary);
    }

    private static async Task<(Account TwdAccount, Account UsdAccount, Category Food, Category Transport, Category Salary)> SeedMultiCurrencyAsync(AppDbContext context)
    {
        var twdAccount = new Account { Name = "現金", NormalizedName = "現金", Type = AccountType.Cash, IconKey = "wallet", Currency = TestDataBuilder.TwdCurrency, CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var usdAccount = new Account { Name = "美元現金", NormalizedName = "美元現金", Type = AccountType.Cash, IconKey = "wallet", Currency = TestDataBuilder.UsdCurrency, CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var food = new Category { Name = "餐飲", NormalizedName = "餐飲", Type = TransactionType.Expense, IconKey = "tag", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var transport = new Category { Name = "交通", NormalizedName = "交通", Type = TransactionType.Expense, IconKey = "tag", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var salary = new Category { Name = "薪資", NormalizedName = "薪資", Type = TransactionType.Income, IconKey = "tag", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        context.AddRange(twdAccount, usdAccount, food, transport, salary);
        await context.SaveChangesAsync();
        return (twdAccount, usdAccount, food, transport, salary);
    }
}
