using System.Diagnostics;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.AccountTransfers;
using BookKeeping2.Services.Accounts;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookKeeping2.Tests.Integration.Performance;

public sealed class AccountTransferPerformanceTests
{
    [Fact]
    public async Task GetBalanceSummariesAsync_aggregates_transfer_balances_without_n_plus_one_under_one_second()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        Account bank = TestDataBuilder.CreateAccount("銀行", TestDataBuilder.TwdCurrency);
        Account cash = TestDataBuilder.CreateAccount("現金", TestDataBuilder.TwdCurrency);
        context.Accounts.AddRange(bank, cash);
        await context.SaveChangesAsync();
        for (int i = 0; i < 10_000; i++)
        {
            context.AccountTransfers.Add(new AccountTransfer
            {
                FromAccountId = bank.Id,
                ToAccountId = cash.Id,
                TransferDate = TestDataBuilder.DefaultToday,
                Currency = TestDataBuilder.TwdCurrency,
                Amount = 1m,
                SubmissionToken = Guid.NewGuid().ToString("N"),
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
                LastChangeSummary = "測試"
            });
        }

        await context.SaveChangesAsync();
        var service = new AccountService(context);
        Stopwatch stopwatch = Stopwatch.StartNew();

        IReadOnlyList<AccountBalanceSummary> balances = await service.GetBalanceSummariesAsync();

        stopwatch.Stop();
        Assert.Equal(-10_000m, balances.Single(balance => balance.AccountId == bank.Id).CurrentBalance);
        Assert.Equal(10_000m, balances.Single(balance => balance.AccountId == cash.Id).CurrentBalance);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1), stopwatch.Elapsed.ToString());
    }
}
