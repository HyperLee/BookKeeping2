using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.AccountTransfers;
using BookKeeping2.Services.AccountTransfers;
using BookKeeping2.Services.Audit;
using BookKeeping2.Tests.TestSupport;
using BookKeeping2.ViewModels.AccountTransfers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BookKeeping2.Tests.Unit.AccountTransfers;

public sealed class AccountTransferServiceTests
{
    [Fact]
    public async Task CreateAsync_persists_valid_transfer_sanitizes_note_and_records_masked_audit()
    {
        await using SqliteTestDatabase database = new();
        await using AppDbContext context = await CreateContextAsync(database);
        AccountTransferService service = CreateService(context);
        (Account from, Account to) = await SeedTransferAccountsAsync(context);

        AccountTransferResult result = await service.CreateAsync(new AccountTransferInputModel
        {
            TransferDate = TestDataBuilder.DefaultToday,
            Currency = TestDataBuilder.TwdCurrency,
            Amount = 1000m,
            FromAccountId = from.Id,
            ToAccountId = to.Id,
            Note = "<b>提款</b><script>alert(1)</script>",
            SubmissionToken = TestDataBuilder.CreateTransferSubmissionToken()
        });

        Assert.True(result.Succeeded);
        AccountTransfer transfer = await context.AccountTransfers.SingleAsync();
        Assert.Equal(1000m, transfer.Amount);
        Assert.Equal(TestDataBuilder.TwdCurrency, transfer.Currency);
        Assert.Equal(from.Id, transfer.FromAccountId);
        Assert.Equal(to.Id, transfer.ToAccountId);
        Assert.DoesNotContain("<script", transfer.Note, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("提款", transfer.Note, StringComparison.Ordinal);
        Assert.Contains(TestDataBuilder.TwdCurrency, transfer.LastChangeSummary);
        Assert.DoesNotContain("1000", transfer.LastChangeSummary, StringComparison.Ordinal);
        Assert.Contains(await context.AuditEvents.ToListAsync(), audit => audit.EventType.ToString() == "TransferCreated");
    }

    [Fact]
    public async Task CreateAsync_rejects_same_account_currency_mismatch_and_future_date()
    {
        await using SqliteTestDatabase database = new();
        await using AppDbContext context = await CreateContextAsync(database);
        AccountTransferService service = CreateService(context);
        (Account from, Account to) = await SeedTransferAccountsAsync(context);
        Account usdAccount = await SeedAccountAsync(context, "美元現金", TestDataBuilder.UsdCurrency);

        AccountTransferResult sameAccount = await service.CreateAsync(CreateInput(from.Id, from.Id));
        AccountTransferResult currencyMismatch = await service.CreateAsync(CreateInput(from.Id, usdAccount.Id));
        AccountTransferResult futureDate = await service.CreateAsync(CreateInput(
            from.Id,
            to.Id,
            transferDate: TestDataBuilder.DefaultToday.AddDays(1),
            submissionToken: TestDataBuilder.CreateTransferSubmissionToken("future")));

        Assert.False(sameAccount.Succeeded);
        Assert.Contains(nameof(AccountTransferInputModel.ToAccountId), sameAccount.Errors.Keys);
        Assert.False(currencyMismatch.Succeeded);
        Assert.Contains(nameof(AccountTransferInputModel.Currency), currencyMismatch.Errors.Keys);
        Assert.False(futureDate.Succeeded);
        Assert.Contains(nameof(AccountTransferInputModel.TransferDate), futureDate.Errors.Keys);
        Assert.Empty(context.AccountTransfers);
    }

    [Fact]
    public async Task CreateAsync_reuses_submission_token_as_success_without_second_insert_but_allows_different_token()
    {
        await using SqliteTestDatabase database = new();
        await using AppDbContext context = await CreateContextAsync(database);
        AccountTransferService service = CreateService(context);
        (Account from, Account to) = await SeedTransferAccountsAsync(context);

        AccountTransferInputModel input = CreateInput(from.Id, to.Id);
        AccountTransferResult first = await service.CreateAsync(input);
        AccountTransferResult duplicate = await service.CreateAsync(input);
        AccountTransferResult sameContentNewToken = await service.CreateAsync(CreateInput(
            from.Id,
            to.Id,
            submissionToken: TestDataBuilder.CreateTransferSubmissionToken("second")));

        Assert.True(first.Succeeded);
        Assert.True(duplicate.Succeeded);
        Assert.True(sameContentNewToken.Succeeded);
        Assert.Equal(2, await context.AccountTransfers.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_revalidates_and_updates_transfer_fields()
    {
        await using SqliteTestDatabase database = new();
        await using AppDbContext context = await CreateContextAsync(database);
        AccountTransferService service = CreateService(context);
        (Account from, Account to) = await SeedTransferAccountsAsync(context);
        Account anotherTo = await SeedAccountAsync(context, "零用金", TestDataBuilder.TwdCurrency);
        AccountTransferResult created = await service.CreateAsync(CreateInput(from.Id, to.Id));

        AccountTransferResult updated = await service.UpdateAsync(created.TransferId!.Value, new AccountTransferInputModel
        {
            TransferDate = TestDataBuilder.DefaultToday.AddDays(-1),
            Currency = TestDataBuilder.TwdCurrency,
            Amount = 250m,
            FromAccountId = from.Id,
            ToAccountId = anotherTo.Id,
            Note = "更新轉帳"
        });

        Assert.True(updated.Succeeded);
        AccountTransfer transfer = await context.AccountTransfers.SingleAsync();
        Assert.Equal(250m, transfer.Amount);
        Assert.Equal(anotherTo.Id, transfer.ToAccountId);
        Assert.Equal("更新轉帳", transfer.Note);
        Assert.Contains(await context.AuditEvents.ToListAsync(), audit => audit.EventType.ToString() == "TransferUpdated");
    }

    [Fact]
    public async Task SoftDeleteAsync_marks_transfer_deleted_and_excludes_missing_or_deleted_records()
    {
        await using SqliteTestDatabase database = new();
        await using AppDbContext context = await CreateContextAsync(database);
        AccountTransferService service = CreateService(context);
        (Account from, Account to) = await SeedTransferAccountsAsync(context);
        AccountTransferResult created = await service.CreateAsync(CreateInput(from.Id, to.Id));

        AccountTransferResult deleted = await service.SoftDeleteAsync(created.TransferId!.Value);
        AccountTransferResult deletedAgain = await service.SoftDeleteAsync(created.TransferId.Value);

        Assert.True(deleted.Succeeded);
        Assert.False(deletedAgain.Succeeded);
        AccountTransfer transfer = await context.AccountTransfers.SingleAsync();
        Assert.True(transfer.IsDeleted);
        Assert.NotNull(transfer.DeletedAtUtc);
        Assert.NotNull(transfer.DeletionSummary);
        Assert.Contains(await context.AuditEvents.ToListAsync(), audit => audit.EventType.ToString() == "TransferDeleted");
    }

    private static async Task<AppDbContext> CreateContextAsync(SqliteTestDatabase database)
    {
        var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    private static AccountTransferService CreateService(AppDbContext context)
    {
        FakeTaipeiDateService dateService = TestDataBuilder.CreateDateService();
        var auditService = new AuditService(context, dateService, NullLogger<AuditService>.Instance);
        return new AccountTransferService(context, dateService, auditService, new AuditLogMaskingPolicy());
    }

    private static AccountTransferInputModel CreateInput(
        long fromAccountId,
        long toAccountId,
        DateOnly? transferDate = null,
        string? submissionToken = null)
    {
        return new AccountTransferInputModel
        {
            TransferDate = transferDate ?? TestDataBuilder.DefaultToday,
            Currency = TestDataBuilder.TwdCurrency,
            Amount = 1000m,
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Note = "測試轉帳",
            SubmissionToken = submissionToken ?? TestDataBuilder.CreateTransferSubmissionToken()
        };
    }

    private static async Task<(Account From, Account To)> SeedTransferAccountsAsync(AppDbContext context)
    {
        Account from = await SeedAccountAsync(context, "銀行", TestDataBuilder.TwdCurrency, openingBalance: 500m);
        Account to = await SeedAccountAsync(context, "現金", TestDataBuilder.TwdCurrency, openingBalance: 10m);
        return (from, to);
    }

    private static async Task<Account> SeedAccountAsync(
        AppDbContext context,
        string name,
        string currency,
        decimal openingBalance = 0m)
    {
        var account = TestDataBuilder.CreateAccount(name, currency);
        account.OpeningBalance = openingBalance;
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        return account;
    }
}
