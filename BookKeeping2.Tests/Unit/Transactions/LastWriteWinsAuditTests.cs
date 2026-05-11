using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Services.Audit;
using BookKeeping2.Services.Transactions;
using BookKeeping2.Tests.TestSupport;
using BookKeeping2.ViewModels.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BookKeeping2.Tests.Unit.Transactions;

public sealed class LastWriteWinsAuditTests
{
    [Fact]
    public async Task UpdateAsync_keeps_last_completed_save_and_records_audit_summary()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        (Category category, Account account) = await SeedCategoryAndAccountAsync(context);
        TransactionService service = CreateService(context);

        TransactionResult create = await service.CreateAsync(CreateInput(category.Id, account.Id, 100m, "第一版"));
        Assert.True(create.Succeeded);

        await service.UpdateAsync(create.TransactionId!.Value, CreateInput(category.Id, account.Id, 150m, "第二版"));
        await service.UpdateAsync(create.TransactionId!.Value, CreateInput(category.Id, account.Id, 200m, "最後版本"));

        var transaction = await context.Transactions.SingleAsync();
        Assert.Equal(200m, transaction.Amount);
        Assert.Equal("最後版本", transaction.Note);
        Assert.True(await context.AuditEvents.CountAsync() >= 3);
    }

    private static TransactionInputModel CreateInput(long categoryId, long accountId, decimal amount, string note)
    {
        return new TransactionInputModel
        {
            TransactionDate = TestDataBuilder.DefaultToday,
            Type = TransactionType.Expense,
            Amount = amount,
            CategoryId = categoryId,
            AccountId = accountId,
            Note = note
        };
    }

    private static TransactionService CreateService(AppDbContext context)
    {
        FakeTaipeiDateService dateService = TestDataBuilder.CreateDateService();
        var auditService = new AuditService(context, dateService, NullLogger<AuditService>.Instance);
        return new TransactionService(context, dateService, auditService, new AuditLogMaskingPolicy());
    }

    private static async Task<(Category Category, Account Account)> SeedCategoryAndAccountAsync(AppDbContext context)
    {
        var category = new Category
        {
            Name = "餐飲",
            NormalizedName = "餐飲",
            Type = TransactionType.Expense,
            IconKey = "tag",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
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
        context.AddRange(category, account);
        await context.SaveChangesAsync();
        return (category, account);
    }
}
