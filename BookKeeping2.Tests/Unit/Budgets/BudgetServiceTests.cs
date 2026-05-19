using System.Diagnostics;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.AccountTransfers;
using BookKeeping2.Models.Audit;
using BookKeeping2.Models.Budgets;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Services.Audit;
using BookKeeping2.Services.Budgets;
using BookKeeping2.Tests.TestSupport;
using BookKeeping2.ViewModels.Budgets;
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
            CreateBudget(food.Id, new DateOnly(2026, 1, 1), 5_000m, TestDataBuilder.TwdCurrency),
            CreateBudget(transport.Id, new DateOnly(2026, 1, 1), 5_000m, TestDataBuilder.TwdCurrency));
        context.Transactions.AddRange(
            CreateTransaction(account.Id, food.Id, TransactionType.Expense, new DateOnly(2026, 1, 5), 4_000m, TestDataBuilder.TwdCurrency),
            CreateTransaction(account.Id, food.Id, TransactionType.Expense, new DateOnly(2025, 12, 31), 999m, TestDataBuilder.TwdCurrency),
            CreateTransaction(account.Id, transport.Id, TransactionType.Expense, new DateOnly(2026, 1, 8), 7_000m, TestDataBuilder.TwdCurrency),
            CreateTransaction(account.Id, salary.Id, TransactionType.Income, new DateOnly(2026, 1, 8), 10_000m, TestDataBuilder.TwdCurrency));
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
            CreateBudget(food.Id, new DateOnly(2026, 5, 1), 1_000m, TestDataBuilder.TwdCurrency),
            CreateBudget(food.Id, new DateOnly(2026, 6, 1), 1_000m, TestDataBuilder.TwdCurrency));
        for (int i = 0; i < 100; i++)
        {
            context.Transactions.Add(CreateTransaction(account.Id, food.Id, TransactionType.Expense, new DateOnly(2026, 5, (i % 28) + 1), 10m, TestDataBuilder.TwdCurrency));
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

    [Fact]
    public async Task SaveAsync_allows_same_month_category_with_different_currency_and_rejects_duplicate_same_currency()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        (_, Category food, _, _) = await SeedAsync(context);
        var service = new BudgetService(context, TestDataBuilder.CreateDateService(), new NullAuditService(), new AuditLogMaskingPolicy());

        BudgetResult twd = await service.SaveAsync(new BudgetInputModel { CategoryId = food.Id, BudgetMonth = new DateOnly(2026, 5, 1), Currency = TestDataBuilder.TwdCurrency, Amount = 5_000m });
        BudgetResult usd = await service.SaveAsync(new BudgetInputModel { CategoryId = food.Id, BudgetMonth = new DateOnly(2026, 5, 1), Currency = TestDataBuilder.UsdCurrency, Amount = 300m });
        BudgetResult duplicateUsd = await service.SaveAsync(new BudgetInputModel { CategoryId = food.Id, BudgetMonth = new DateOnly(2026, 5, 1), Currency = TestDataBuilder.UsdCurrency, Amount = 400m });

        Assert.True(twd.Succeeded);
        Assert.True(usd.Succeeded);
        Assert.False(duplicateUsd.Succeeded);
        Assert.Contains(nameof(BudgetInputModel.Currency), duplicateUsd.Errors.Keys);
        Assert.Equal(2, await context.Budgets.CountAsync());
    }

    [Fact]
    public async Task ListMonthlyAsync_calculates_progress_by_matching_currency_only()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        (Account account, Category food, _, _) = await SeedAsync(context);
        context.Budgets.AddRange(
            CreateBudget(food.Id, new DateOnly(2026, 5, 1), 5_000m, TestDataBuilder.TwdCurrency),
            CreateBudget(food.Id, new DateOnly(2026, 5, 1), 300m, TestDataBuilder.UsdCurrency));
        context.Transactions.AddRange(
            CreateTransaction(account.Id, food.Id, TransactionType.Expense, new DateOnly(2026, 5, 5), 1_000m, TestDataBuilder.TwdCurrency),
            CreateTransaction(account.Id, food.Id, TransactionType.Expense, new DateOnly(2026, 5, 5), 100m, TestDataBuilder.UsdCurrency));
        await context.SaveChangesAsync();
        var service = new BudgetService(context, TestDataBuilder.CreateDateService(), new NullAuditService(), new AuditLogMaskingPolicy());

        IReadOnlyList<BudgetStatusViewModel> budgets = await service.ListMonthlyAsync(new DateOnly(2026, 5, 1));

        BudgetStatusViewModel twd = Assert.Single(budgets, budget => budget.Currency == TestDataBuilder.TwdCurrency);
        BudgetStatusViewModel usd = Assert.Single(budgets, budget => budget.Currency == TestDataBuilder.UsdCurrency);
        Assert.Equal(1_000m, twd.SpentAmount);
        Assert.Equal(4_000m, twd.RemainingAmount);
        Assert.Equal(100m, usd.SpentAmount);
        Assert.Equal(200m, usd.RemainingAmount);
    }

    [Fact]
    public async Task ListMonthlyAsync_excludes_transfer_payments_from_budget_usage_and_alert_state()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        (Account bank, Category food, _, _) = await SeedAsync(context);
        Account creditCard = new() { Name = "信用卡", NormalizedName = "信用卡", Type = AccountType.CreditCard, IconKey = "credit-card", Currency = TestDataBuilder.TwdCurrency, CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        context.Accounts.Add(creditCard);
        await context.SaveChangesAsync();
        context.Budgets.Add(CreateBudget(food.Id, new DateOnly(2026, 5, 1), 1_000m, TestDataBuilder.TwdCurrency));
        context.Transactions.Add(CreateTransaction(bank.Id, food.Id, TransactionType.Expense, new DateOnly(2026, 5, 5), 100m, TestDataBuilder.TwdCurrency));
        context.AccountTransfers.Add(new AccountTransfer
        {
            TransferDate = new DateOnly(2026, 5, 6),
            Currency = TestDataBuilder.TwdCurrency,
            Amount = 2_000m,
            FromAccountId = bank.Id,
            ToAccountId = creditCard.Id,
            SubmissionToken = Guid.NewGuid().ToString("N"),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "信用卡繳款"
        });
        await context.SaveChangesAsync();
        var service = new BudgetService(context, TestDataBuilder.CreateDateService(), new NullAuditService(), new AuditLogMaskingPolicy());

        BudgetStatusViewModel budget = Assert.Single(await service.ListMonthlyAsync(new DateOnly(2026, 5, 1)));

        Assert.Equal(100m, budget.SpentAmount);
        Assert.Equal(900m, budget.RemainingAmount);
        Assert.Equal(BudgetAlertState.Normal, budget.AlertState);
    }

    private static Budget CreateBudget(long categoryId, DateOnly month, decimal amount, string currency)
    {
        return new Budget
        {
            CategoryId = categoryId,
            BudgetMonth = month,
            Currency = currency,
            Amount = amount,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    private static Transaction CreateTransaction(long accountId, long categoryId, TransactionType type, DateOnly date, decimal amount, string currency)
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
