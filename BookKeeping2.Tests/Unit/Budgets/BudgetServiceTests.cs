using System.Diagnostics;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Audit;
using BookKeeping2.Models.Budgets;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Services.Audit;
using BookKeeping2.Services.Budgets;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookKeeping2.Tests.Unit.Budgets;

public sealed class BudgetServiceTests
{
    [Fact]
    public async Task ListMonthlyAsync_calculates_usage_remaining_overspent_and_alert_states()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        (Account account, Category food, Category transport, Category salary) = await SeedAsync(context);
        context.Budgets.AddRange(
            CreateBudget(food.Id, new DateOnly(2026, 1, 1), 5_000m),
            CreateBudget(transport.Id, new DateOnly(2026, 1, 1), 5_000m));
        context.Transactions.AddRange(
            CreateTransaction(account.Id, food.Id, TransactionType.Expense, new DateOnly(2026, 1, 5), 4_000m),
            CreateTransaction(account.Id, food.Id, TransactionType.Expense, new DateOnly(2025, 12, 31), 999m),
            CreateTransaction(account.Id, transport.Id, TransactionType.Expense, new DateOnly(2026, 1, 8), 7_000m),
            CreateTransaction(account.Id, salary.Id, TransactionType.Income, new DateOnly(2026, 1, 8), 10_000m));
        await context.SaveChangesAsync();
        var service = new BudgetService(context, TestDataBuilder.CreateDateService(), new NullAuditService(), new AuditLogMaskingPolicy());

        var budgets = await service.ListMonthlyAsync(new DateOnly(2026, 1, 1));

        var foodBudget = Assert.Single(budgets, budget => budget.CategoryName == "餐飲");
        Assert.Equal(5_000m, foodBudget.Amount);
        Assert.Equal(4_000m, foodBudget.SpentAmount);
        Assert.Equal(0.8m, foodBudget.UsageRate);
        Assert.Equal(1_000m, foodBudget.RemainingAmount);
        Assert.Equal(0m, foodBudget.OverspentAmount);
        Assert.Equal(BudgetAlertState.NearLimit, foodBudget.AlertState);

        var transportBudget = Assert.Single(budgets, budget => budget.CategoryName == "交通");
        Assert.Equal(7_000m, transportBudget.SpentAmount);
        Assert.Equal(1.4m, transportBudget.UsageRate);
        Assert.Equal(0m, transportBudget.RemainingAmount);
        Assert.Equal(2_000m, transportBudget.OverspentAmount);
        Assert.Equal(BudgetAlertState.Exceeded, transportBudget.AlertState);
    }

    [Fact]
    public async Task ListMonthlyAsync_recalculates_by_month_and_completes_alert_calculation_under_one_second()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        (Account account, Category food, _, _) = await SeedAsync(context);
        context.Budgets.AddRange(
            CreateBudget(food.Id, new DateOnly(2026, 5, 1), 1_000m),
            CreateBudget(food.Id, new DateOnly(2026, 6, 1), 1_000m));
        for (int i = 0; i < 100; i++)
        {
            context.Transactions.Add(CreateTransaction(account.Id, food.Id, TransactionType.Expense, new DateOnly(2026, 5, (i % 28) + 1), 10m));
        }
        await context.SaveChangesAsync();
        var service = new BudgetService(context, TestDataBuilder.CreateDateService(), new NullAuditService(), new AuditLogMaskingPolicy());
        Stopwatch stopwatch = Stopwatch.StartNew();

        var mayBudget = Assert.Single(await service.ListMonthlyAsync(new DateOnly(2026, 5, 1)));
        var juneBudget = Assert.Single(await service.ListMonthlyAsync(new DateOnly(2026, 6, 1)));

        stopwatch.Stop();
        Assert.Equal(1_000m, mayBudget.SpentAmount);
        Assert.Equal(BudgetAlertState.NearLimit, mayBudget.AlertState);
        Assert.Equal(0m, juneBudget.SpentAmount);
        Assert.Equal(BudgetAlertState.Normal, juneBudget.AlertState);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1));
    }

    private static Budget CreateBudget(long categoryId, DateOnly month, decimal amount)
    {
        return new Budget
        {
            CategoryId = categoryId,
            BudgetMonth = month,
            Amount = amount,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    private static Transaction CreateTransaction(long accountId, long categoryId, TransactionType type, DateOnly date, decimal amount)
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

    private sealed class NullAuditService : IAuditService
    {
        public Task RecordAsync(
            AuditEventType eventType,
            string entityType,
            string? entityId,
            string summary,
            string severity = "Information",
            string? correlationId = null,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
