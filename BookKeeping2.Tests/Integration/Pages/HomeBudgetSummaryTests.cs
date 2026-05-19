using System.Net;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.AccountTransfers;
using BookKeeping2.Models.Budgets;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookKeeping2.Tests.Integration.Pages;

public sealed class HomeBudgetSummaryTests
{
    [Fact]
    public async Task Home_page_shows_current_month_budget_progress_and_alert_within_one_second()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();
        await SeedBudgetAndSpendingAsync(factory);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        string page = WebUtility.HtmlDecode(await client.GetStringAsync("/", timeout.Token));

        Assert.Contains("預算進度", page);
        Assert.Contains("餐飲", page);
        Assert.Contains("80.00%", page);
        Assert.Contains("接近預算上限", page);
    }

    [Fact]
    public async Task Home_page_shows_month_summary_buckets_by_currency_without_cross_currency_total()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();
        await SeedMultiCurrencySummaryAsync(factory);

        string page = WebUtility.HtmlDecode(await client.GetStringAsync("/"));

        Assert.Contains("TWD", page);
        Assert.Contains("USD", page);
        Assert.Contains("TWD 1,000.00", page);
        Assert.Contains("USD 500.00", page);
        Assert.DoesNotContain("1,500", page);
    }

    [Fact]
    public async Task Home_page_keeps_income_expense_summary_transaction_only_while_balances_include_transfers()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();
        await SeedTransferAwareHomeSummaryAsync(factory);

        string page = WebUtility.HtmlDecode(await client.GetStringAsync("/"));

        Assert.Contains("TWD 1,000.00", page);
        Assert.Contains("TWD 300.00", page);
        Assert.DoesNotContain("TWD 2,000.00", page);
        Assert.Contains("銀行", page);
        Assert.Contains("TWD 3,700.00", page);
        Assert.Contains("信用卡", page);
        Assert.Contains("TWD 1,800.00", page);
    }

    private static async Task SeedBudgetAndSpendingAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Category food = await context.Categories.FirstAsync(category => category.Type == TransactionType.Expense && category.Name == "餐飲");
        var account = new Account
        {
            Name = "現金",
            NormalizedName = "現金",
            Type = AccountType.Cash,
            IconKey = "wallet",
            Currency = "TWD",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        context.Budgets.Add(new Budget
        {
            CategoryId = food.Id,
            BudgetMonth = new DateOnly(2026, 5, 1),
            Amount = 5_000m,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });
        context.Transactions.Add(new Transaction
        {
            AccountId = account.Id,
            CategoryId = food.Id,
            Type = TransactionType.Expense,
            TransactionDate = new DateOnly(2026, 5, 5),
            Amount = 4_000m,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "測試"
        });
        await context.SaveChangesAsync();
    }

    private static async Task SeedTransferAwareHomeSummaryAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Category salary = await context.Categories.FirstAsync(category => category.Type == TransactionType.Income && category.Name == "薪資");
        Category food = await context.Categories.FirstAsync(category => category.Type == TransactionType.Expense && category.Name == "餐飲");
        var bank = new Account { Name = "銀行", NormalizedName = "銀行", Type = AccountType.Bank, IconKey = "bank", Currency = TestDataBuilder.TwdCurrency, OpeningBalance = 5_000m, CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var creditCard = new Account { Name = "信用卡", NormalizedName = "信用卡", Type = AccountType.CreditCard, IconKey = "credit-card", Currency = TestDataBuilder.TwdCurrency, OpeningBalance = -200m, CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        context.Accounts.AddRange(bank, creditCard);
        await context.SaveChangesAsync();
        context.Transactions.AddRange(
            CreateTransaction(bank.Id, salary.Id, TransactionType.Income, 1_000m, TestDataBuilder.TwdCurrency),
            CreateTransaction(bank.Id, food.Id, TransactionType.Expense, 300m, TestDataBuilder.TwdCurrency));
        context.AccountTransfers.Add(new AccountTransfer
        {
            TransferDate = new DateOnly(2026, 5, 8),
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
    }

    private static async Task SeedMultiCurrencySummaryAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Category salary = await context.Categories.FirstAsync(category => category.Type == TransactionType.Income && category.Name == "薪資");
        Category food = await context.Categories.FirstAsync(category => category.Type == TransactionType.Expense && category.Name == "餐飲");
        var twdAccount = new Account { Name = "現金", NormalizedName = "現金", Type = AccountType.Cash, IconKey = "wallet", Currency = TestDataBuilder.TwdCurrency, CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var usdAccount = new Account { Name = "美元現金", NormalizedName = "美元現金", Type = AccountType.Cash, IconKey = "wallet", Currency = TestDataBuilder.UsdCurrency, CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        context.Accounts.AddRange(twdAccount, usdAccount);
        await context.SaveChangesAsync();

        context.Transactions.AddRange(
            CreateTransaction(twdAccount.Id, salary.Id, TransactionType.Income, 1_000m, TestDataBuilder.TwdCurrency),
            CreateTransaction(usdAccount.Id, salary.Id, TransactionType.Income, 500m, TestDataBuilder.UsdCurrency),
            CreateTransaction(twdAccount.Id, food.Id, TransactionType.Expense, 100m, TestDataBuilder.TwdCurrency),
            CreateTransaction(usdAccount.Id, food.Id, TransactionType.Expense, 50m, TestDataBuilder.UsdCurrency));
        await context.SaveChangesAsync();
    }

    private static Transaction CreateTransaction(long accountId, long categoryId, TransactionType type, decimal amount, string currency)
    {
        return new Transaction
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Type = type,
            TransactionDate = new DateOnly(2026, 5, 5),
            Currency = currency,
            Amount = amount,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "測試"
        };
    }
}
