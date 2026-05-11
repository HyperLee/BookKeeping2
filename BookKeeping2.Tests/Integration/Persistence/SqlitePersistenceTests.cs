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

namespace BookKeeping2.Tests.Integration.Persistence;

public sealed class SqlitePersistenceTests
{
    [Fact]
    public async Task Transaction_persists_after_recreating_db_context_for_same_sqlite_file()
    {
        string databasePath = Path.Combine(AppContext.BaseDirectory, "Persistence", $"{Guid.NewGuid():N}.db");
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
        DbContextOptions<AppDbContext> options = CreateOptions(databasePath);

        await using (var context = new AppDbContext(options))
        {
            await context.Database.MigrateAsync();
            (Category category, Account account) = await SeedCategoryAndAccountAsync(context);
            TransactionService service = CreateService(context);

            TransactionResult result = await service.CreateAsync(new TransactionInputModel
            {
                TransactionDate = TestDataBuilder.DefaultToday,
                Type = TransactionType.Expense,
                Amount = 150m,
                CategoryId = category.Id,
                AccountId = account.Id,
                Note = "跨內容保存"
            });

            Assert.True(result.Succeeded);
        }

        await using (var context = new AppDbContext(options))
        {
            Assert.True(await context.Transactions.AnyAsync(transaction => transaction.Note == "跨內容保存"));
        }
    }

    private static DbContextOptions<AppDbContext> CreateOptions(string databasePath)
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;
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
