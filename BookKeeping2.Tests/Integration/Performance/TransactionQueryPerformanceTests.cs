using System.Diagnostics;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Services.Transactions;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookKeeping2.Tests.Integration.Performance;

public sealed class TransactionQueryPerformanceTests
{
    [Fact]
    public async Task SearchAsync_filters_10000_transactions_under_two_seconds()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        (Account account, Category food) = await SeedAsync(context);
        for (int i = 0; i < 10_000; i++)
        {
            context.Transactions.Add(new Transaction
            {
                AccountId = account.Id,
                CategoryId = food.Id,
                Type = TransactionType.Expense,
                TransactionDate = new DateOnly(2026, 1, (i % 28) + 1),
                Amount = i == 9_999 ? 777m : 10m,
                Note = i == 9_999 ? "唯一查詢目標" : $"一般資料 {i}",
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
                LastChangeSummary = "測試"
            });
        }
        await context.SaveChangesAsync();
        var service = new TransactionQueryService(context);
        Stopwatch stopwatch = Stopwatch.StartNew();

        var result = await service.SearchAsync(new TransactionQuery { Keyword = "唯一查詢目標", Page = 1, PageSize = 20 });

        stopwatch.Stop();
        var item = Assert.Single(result.Items);
        Assert.Equal(777m, item.Amount);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2));
    }

    private static async Task<(Account Account, Category Food)> SeedAsync(AppDbContext context)
    {
        var account = new Account { Name = "現金", NormalizedName = "現金", Type = AccountType.Cash, IconKey = "wallet", Currency = "TWD", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var food = new Category { Name = "餐飲", NormalizedName = "餐飲", Type = TransactionType.Expense, IconKey = "tag", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        context.AddRange(account, food);
        await context.SaveChangesAsync();
        return (account, food);
    }
}
