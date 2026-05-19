using System.Diagnostics;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.AccountTransfers;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Services.Transactions;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookKeeping2.Tests.Integration.Performance;

public sealed class TransactionTimelinePerformanceTests
{
    [Fact]
    public async Task SearchAsync_filters_10000_mixed_transaction_and_transfer_rows_under_two_seconds()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        Account cash = TestDataBuilder.CreateAccount("現金", TestDataBuilder.TwdCurrency);
        Account bank = TestDataBuilder.CreateAccount("銀行", TestDataBuilder.TwdCurrency);
        var food = new Category { Name = "餐飲", NormalizedName = "餐飲", Type = TransactionType.Expense, IconKey = "tag", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        context.AddRange(cash, bank, food);
        await context.SaveChangesAsync();

        for (int i = 0; i < 5_000; i++)
        {
            context.Transactions.Add(new Transaction
            {
                AccountId = i % 2 == 0 ? cash.Id : bank.Id,
                CategoryId = food.Id,
                Type = TransactionType.Expense,
                TransactionDate = TestDataBuilder.DefaultToday.AddDays(-(i % 28)),
                Currency = TestDataBuilder.TwdCurrency,
                Amount = 10m,
                Note = "效能交易",
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
                LastChangeSummary = "測試"
            });
            context.AccountTransfers.Add(new AccountTransfer
            {
                FromAccountId = bank.Id,
                ToAccountId = cash.Id,
                TransferDate = TestDataBuilder.DefaultToday.AddDays(-(i % 28)),
                Currency = TestDataBuilder.TwdCurrency,
                Amount = 10m,
                Note = "效能轉帳",
                SubmissionToken = Guid.NewGuid().ToString("N"),
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
                LastChangeSummary = "測試"
            });
        }

        await context.SaveChangesAsync();
        var service = new TransactionQueryService(context);
        Stopwatch stopwatch = Stopwatch.StartNew();

        var result = await service.SearchAsync(new TransactionQuery { AccountId = cash.Id, PageSize = 50 });

        stopwatch.Stop();
        Assert.True(result.TotalCount > 0);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2), stopwatch.Elapsed.ToString());
    }
}
