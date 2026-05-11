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

public sealed class TransactionFilterPageTests
{
    [Fact]
    public async Task Transactions_page_filters_by_multiple_fields_and_shows_only_matching_rows()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();
        (long categoryId, long accountId) = await SeedAsync(factory);

        string page = WebUtility.HtmlDecode(await client.GetStringAsync(
            $"/Transactions?Filter.StartDate=2026-02-01&Filter.EndDate=2026-02-28&Filter.CategoryId={categoryId}&Filter.AccountId={accountId}&Filter.MinAmount=100&Filter.MaxAmount=200&Filter.Keyword=便當"));

        Assert.Contains("篩選結果 1 筆", page);
        Assert.Contains("午餐便當", page);
        Assert.DoesNotContain("晚餐聚會", page);
        Assert.DoesNotContain("捷運", page);
    }

    private static async Task<(long CategoryId, long AccountId)> SeedAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Category food = await context.Categories.FirstAsync(category => category.Type == TransactionType.Expense && category.Name == "餐飲");
        Category transport = await context.Categories.FirstAsync(category => category.Type == TransactionType.Expense && category.Name == "交通");
        var account = new Account { Name = "現金", NormalizedName = "現金", Type = AccountType.Cash, IconKey = "wallet", Currency = "TWD", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        context.Transactions.AddRange(
            Create(account.Id, food.Id, 150m, "午餐便當"),
            Create(account.Id, food.Id, 500m, "晚餐聚會"),
            Create(account.Id, transport.Id, 120m, "捷運"));
        await context.SaveChangesAsync();
        return (food.Id, account.Id);
    }

    private static Transaction Create(long accountId, long categoryId, decimal amount, string note)
    {
        return new Transaction
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Type = TransactionType.Expense,
            TransactionDate = new DateOnly(2026, 2, 10),
            Amount = amount,
            Note = note,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "測試"
        };
    }
}
