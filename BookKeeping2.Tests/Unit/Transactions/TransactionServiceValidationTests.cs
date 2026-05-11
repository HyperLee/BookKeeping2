using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Services.Audit;
using BookKeeping2.Services.Transactions;
using BookKeeping2.Tests.TestSupport;
using BookKeeping2.ViewModels.Transactions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BookKeeping2.Tests.Unit.Transactions;

public sealed class TransactionServiceValidationTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1.234)]
    [InlineData(1_000_000_000)]
    public async Task CreateAsync_rejects_invalid_amounts(decimal amount)
    {
        await using SqliteTestDatabase database = new();
        await using AppDbContext context = await CreateContextAsync(database);
        TransactionService service = CreateService(context);
        (Category category, Account account) = await SeedCategoryAndAccountAsync(context);

        TransactionResult result = await service.CreateAsync(new TransactionInputModel
        {
            TransactionDate = TestDataBuilder.DefaultToday,
            Type = TransactionType.Expense,
            Amount = amount,
            CategoryId = category.Id,
            AccountId = account.Id
        });

        Assert.False(result.Succeeded);
        Assert.Contains(nameof(TransactionInputModel.Amount), result.Errors.Keys);
    }

    [Fact]
    public async Task CreateAsync_rejects_future_transaction_date_in_taipei_calendar()
    {
        await using SqliteTestDatabase database = new();
        await using AppDbContext context = await CreateContextAsync(database);
        TransactionService service = CreateService(context);
        (Category category, Account account) = await SeedCategoryAndAccountAsync(context);

        TransactionResult result = await service.CreateAsync(new TransactionInputModel
        {
            TransactionDate = TestDataBuilder.DefaultToday.AddDays(1),
            Type = TransactionType.Expense,
            Amount = 150m,
            CategoryId = category.Id,
            AccountId = account.Id
        });

        Assert.False(result.Succeeded);
        Assert.Contains(nameof(TransactionInputModel.TransactionDate), result.Errors.Keys);
    }

    [Fact]
    public async Task CreateAsync_rejects_category_that_does_not_match_transaction_type()
    {
        await using SqliteTestDatabase database = new();
        await using AppDbContext context = await CreateContextAsync(database);
        TransactionService service = CreateService(context);
        Account account = await SeedAccountAsync(context);
        Category incomeCategory = await SeedCategoryAsync(context, "薪資", TransactionType.Income);

        TransactionResult result = await service.CreateAsync(new TransactionInputModel
        {
            TransactionDate = TestDataBuilder.DefaultToday,
            Type = TransactionType.Expense,
            Amount = 150m,
            CategoryId = incomeCategory.Id,
            AccountId = account.Id
        });

        Assert.False(result.Succeeded);
        Assert.Contains(nameof(TransactionInputModel.CategoryId), result.Errors.Keys);
    }

    private static async Task<AppDbContext> CreateContextAsync(SqliteTestDatabase database)
    {
        var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    private static TransactionService CreateService(AppDbContext context)
    {
        FakeTaipeiDateService dateService = TestDataBuilder.CreateDateService();
        var auditService = new AuditService(context, dateService, NullLogger<AuditService>.Instance);
        return new TransactionService(context, dateService, auditService, new AuditLogMaskingPolicy());
    }

    private static async Task<(Category Category, Account Account)> SeedCategoryAndAccountAsync(AppDbContext context)
    {
        Category category = await SeedCategoryAsync(context, "餐飲", TransactionType.Expense);
        Account account = await SeedAccountAsync(context);
        return (category, account);
    }

    private static async Task<Category> SeedCategoryAsync(AppDbContext context, string name, TransactionType type)
    {
        var category = new Category
        {
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            Type = type,
            IconKey = "tag",
            DisplayOrder = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category;
    }

    private static async Task<Account> SeedAccountAsync(AppDbContext context)
    {
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
        return account;
    }
}
