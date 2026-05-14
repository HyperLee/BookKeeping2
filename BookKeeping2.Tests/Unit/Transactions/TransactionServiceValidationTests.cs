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
            Currency = TestDataBuilder.TwdCurrency,
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
            Currency = TestDataBuilder.TwdCurrency,
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
            Currency = TestDataBuilder.TwdCurrency,
            Amount = 150m,
            CategoryId = incomeCategory.Id,
            AccountId = account.Id
        });

        Assert.False(result.Succeeded);
        Assert.Contains(nameof(TransactionInputModel.CategoryId), result.Errors.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("AUD")]
    public async Task CreateAsync_rejects_missing_or_unsupported_currency(string? currency)
    {
        await using SqliteTestDatabase database = new();
        await using AppDbContext context = await CreateContextAsync(database);
        TransactionService service = CreateService(context);
        (Category category, Account account) = await SeedCategoryAndAccountAsync(context);

        TransactionResult result = await service.CreateAsync(new TransactionInputModel
        {
            TransactionDate = TestDataBuilder.DefaultToday,
            Type = TransactionType.Expense,
            Currency = currency,
            Amount = 150m,
            CategoryId = category.Id,
            AccountId = account.Id
        });

        Assert.False(result.Succeeded);
        Assert.Contains(nameof(TransactionInputModel.Currency), result.Errors.Keys);
    }

    [Fact]
    public async Task CreateAsync_rejects_account_currency_mismatch()
    {
        await using SqliteTestDatabase database = new();
        await using AppDbContext context = await CreateContextAsync(database);
        TransactionService service = CreateService(context);
        (Category category, Account twdAccount) = await SeedCategoryAndAccountAsync(context);

        TransactionResult result = await service.CreateAsync(new TransactionInputModel
        {
            TransactionDate = TestDataBuilder.DefaultToday,
            Type = TransactionType.Expense,
            Currency = TestDataBuilder.UsdCurrency,
            Amount = 150m,
            CategoryId = category.Id,
            AccountId = twdAccount.Id
        });

        Assert.False(result.Succeeded);
        Assert.Contains(nameof(TransactionInputModel.AccountId), result.Errors.Keys);
    }

    [Fact]
    public async Task CreateAsync_allows_same_date_category_amount_with_different_currency_and_duplicate_detection_includes_currency()
    {
        await using SqliteTestDatabase database = new();
        await using AppDbContext context = await CreateContextAsync(database);
        TransactionService service = CreateService(context);
        Category category = await SeedCategoryAsync(context, "餐飲", TransactionType.Expense);
        Account twdAccount = await SeedAccountAsync(context, "現金", TestDataBuilder.TwdCurrency);
        Account usdAccount = await SeedAccountAsync(context, "美元現金", TestDataBuilder.UsdCurrency);

        TransactionResult twdResult = await service.CreateAsync(CreateInput(category.Id, twdAccount.Id, TestDataBuilder.TwdCurrency));
        TransactionResult usdResult = await service.CreateAsync(CreateInput(category.Id, usdAccount.Id, TestDataBuilder.UsdCurrency));
        TransactionResult duplicateUsdResult = await service.CreateAsync(CreateInput(category.Id, usdAccount.Id, TestDataBuilder.UsdCurrency));

        Assert.True(twdResult.Succeeded);
        Assert.True(usdResult.Succeeded);
        Assert.True(duplicateUsdResult.Succeeded);
        Assert.Equal(2, await context.Transactions.CountAsync());
        Assert.Contains(await context.Transactions.ToListAsync(), transaction => transaction.Currency == TestDataBuilder.TwdCurrency);
        Assert.Contains(await context.Transactions.ToListAsync(), transaction => transaction.Currency == TestDataBuilder.UsdCurrency);
    }

    [Fact]
    public async Task CreateAsync_persists_jpy_currency_and_includes_currency_in_audit_summary()
    {
        await using SqliteTestDatabase database = new();
        await using AppDbContext context = await CreateContextAsync(database);
        TransactionService service = CreateService(context);
        Category category = await SeedCategoryAsync(context, "餐飲", TransactionType.Expense);
        Account account = await SeedAccountAsync(context, "日幣現金", TestDataBuilder.JpyCurrency);

        TransactionResult result = await service.CreateAsync(new TransactionInputModel
        {
            TransactionDate = TestDataBuilder.DefaultToday,
            Type = TransactionType.Expense,
            Currency = " jpy ",
            Amount = 150.25m,
            CategoryId = category.Id,
            AccountId = account.Id,
            Note = "Tokyo lunch 原文"
        });

        Assert.True(result.Succeeded);
        var transaction = await context.Transactions.SingleAsync();
        Assert.Equal(TestDataBuilder.JpyCurrency, transaction.Currency);
        Assert.Equal("Tokyo lunch 原文", transaction.Note);
        Assert.Contains(TestDataBuilder.JpyCurrency, transaction.LastChangeSummary);
        Assert.Contains(TestDataBuilder.JpyCurrency, (await context.AuditEvents.SingleAsync()).Summary);
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

    private static TransactionInputModel CreateInput(long categoryId, long accountId, string currency)
    {
        return new TransactionInputModel
        {
            TransactionDate = TestDataBuilder.DefaultToday,
            Type = TransactionType.Expense,
            Currency = currency,
            Amount = 100m,
            CategoryId = categoryId,
            AccountId = accountId,
            Note = "同日同分類同金額"
        };
    }

    private static async Task<Account> SeedAccountAsync(AppDbContext context, string name = "現金", string currency = TestDataBuilder.TwdCurrency)
    {
        var account = new Account
        {
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            Type = AccountType.Cash,
            IconKey = "wallet",
            Currency = currency,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        return account;
    }
}
