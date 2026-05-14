using System.Net;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookKeeping2.Tests.Integration.Pages;

public sealed class ReportsPageTests
{
    [Fact]
    public async Task Reports_page_shows_blank_state_when_month_has_no_transactions()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();

        string page = await client.GetStringAsync("/Reports?Month=2026-03");

        Assert.Contains("本月尚無紀錄", WebUtility.HtmlDecode(page));
        Assert.Contains("新增交易", WebUtility.HtmlDecode(page));
    }

    [Fact]
    public async Task Reports_page_shows_currency_buckets_and_chart_payload_without_cross_currency_total()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();
        await SeedMultiCurrencyReportAsync(factory);

        string page = WebUtility.HtmlDecode(await client.GetStringAsync("/Reports?Month=2026-05"));

        Assert.Contains("TWD", page);
        Assert.Contains("USD", page);
        Assert.Contains("TWD 1,000.00", page);
        Assert.Contains("USD 500.00", page);
        Assert.DoesNotContain("1,500", page);
        Assert.Contains("\"Currency\":\"TWD\"", page);
        Assert.Contains("\"Currency\":\"USD\"", page);
    }

    private static async Task SeedMultiCurrencyReportAsync(BookKeepingWebApplicationFactory factory)
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
