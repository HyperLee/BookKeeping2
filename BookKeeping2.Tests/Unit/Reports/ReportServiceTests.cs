using System.Diagnostics;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Services.Reports;
using BookKeeping2.Tests.TestSupport;
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

        Assert.Equal(1_000m, report.TotalIncome);
        Assert.Equal(400m, report.TotalExpense);
        Assert.Equal(600m, report.Balance);
        Assert.Equal(2, report.CategoryShares.Count);
        Assert.Contains(report.CategoryShares, share => share.CategoryName == "交通" && share.Amount == 300m && share.Percentage == 75m);
        Assert.DoesNotContain(report.CategoryShares, share => share.Amount == 999m);
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
        Assert.Equal(1_000m, report.TotalExpense);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2));
    }

    private static Transaction Create(long accountId, long categoryId, TransactionType type, DateOnly date, decimal amount)
    {
        return new Transaction
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Type = type,
            TransactionDate = date,
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
}
