using System.Diagnostics;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.AccountTransfers;
using BookKeeping2.Services.Accounts;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookKeeping2.Tests.Unit.Accounts;

public sealed class AccountTransferBalanceTests
{
    [Fact]
    public async Task GetBalanceSummariesAsync_includes_outgoing_and_incoming_transfers_and_allows_negative_balance()
    {
        await using SqliteTestDatabase database = new();
        await using AppDbContext context = await CreateContextAsync(database);
        Account from = await SeedAccountAsync(context, "銀行", 500m);
        Account to = await SeedAccountAsync(context, "現金", 10m);
        await SeedTransferAsync(context, from.Id, to.Id, 1000m);
        var service = new AccountService(context);

        IReadOnlyList<AccountBalanceSummary> balances = await service.GetBalanceSummariesAsync();

        Assert.Equal(-500m, balances.Single(balance => balance.AccountId == from.Id).CurrentBalance);
        Assert.Equal(1010m, balances.Single(balance => balance.AccountId == to.Id).CurrentBalance);
    }

    [Fact]
    public async Task GetBalanceSummariesAsync_reflects_edit_and_soft_delete_without_stale_balance()
    {
        await using SqliteTestDatabase database = new();
        await using AppDbContext context = await CreateContextAsync(database);
        Account from = await SeedAccountAsync(context, "銀行", 500m);
        Account to = await SeedAccountAsync(context, "現金", 10m);
        Account newTo = await SeedAccountAsync(context, "零用金", 1m);
        AccountTransfer transfer = await SeedTransferAsync(context, from.Id, to.Id, 100m);
        var service = new AccountService(context);

        transfer.Amount = 250m;
        transfer.ToAccountId = newTo.Id;
        transfer.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync();
        Stopwatch editWatch = Stopwatch.StartNew();
        IReadOnlyList<AccountBalanceSummary> editedBalances = await service.GetBalanceSummariesAsync();
        editWatch.Stop();

        transfer.IsDeleted = true;
        transfer.DeletedAtUtc = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync();
        Stopwatch deleteWatch = Stopwatch.StartNew();
        IReadOnlyList<AccountBalanceSummary> deletedBalances = await service.GetBalanceSummariesAsync();
        deleteWatch.Stop();

        Assert.Equal(250m, editedBalances.Single(balance => balance.AccountId == from.Id).CurrentBalance);
        Assert.Equal(10m, editedBalances.Single(balance => balance.AccountId == to.Id).CurrentBalance);
        Assert.Equal(251m, editedBalances.Single(balance => balance.AccountId == newTo.Id).CurrentBalance);
        Assert.Equal(500m, deletedBalances.Single(balance => balance.AccountId == from.Id).CurrentBalance);
        Assert.Equal(10m, deletedBalances.Single(balance => balance.AccountId == to.Id).CurrentBalance);
        Assert.Equal(1m, deletedBalances.Single(balance => balance.AccountId == newTo.Id).CurrentBalance);
        Assert.True(editWatch.Elapsed < TimeSpan.FromSeconds(1));
        Assert.True(deleteWatch.Elapsed < TimeSpan.FromSeconds(1));
    }

    private static async Task<AppDbContext> CreateContextAsync(SqliteTestDatabase database)
    {
        var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    private static async Task<Account> SeedAccountAsync(AppDbContext context, string name, decimal openingBalance)
    {
        var account = TestDataBuilder.CreateAccount(name, TestDataBuilder.TwdCurrency);
        account.OpeningBalance = openingBalance;
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        return account;
    }

    private static async Task<AccountTransfer> SeedTransferAsync(AppDbContext context, long fromAccountId, long toAccountId, decimal amount)
    {
        var transfer = new AccountTransfer
        {
            TransferDate = TestDataBuilder.DefaultToday,
            Currency = TestDataBuilder.TwdCurrency,
            Amount = amount,
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            SubmissionToken = Guid.NewGuid().ToString("N"),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "test transfer"
        };
        context.AccountTransfers.Add(transfer);
        await context.SaveChangesAsync();
        return transfer;
    }
}
