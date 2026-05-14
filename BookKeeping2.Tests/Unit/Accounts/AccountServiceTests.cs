using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Services.Accounts;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookKeeping2.Tests.Unit.Accounts;

public sealed class AccountServiceTests
{
    [Fact]
    public async Task CreateAsync_rejects_duplicate_normalized_name()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        var service = new AccountService(context);

        AccountResult first = await service.CreateAsync(" 現金 ", AccountType.Cash, 0m, TestDataBuilder.TwdCurrency);
        AccountResult duplicate = await service.CreateAsync("現金", AccountType.Bank, 0m, TestDataBuilder.UsdCurrency);

        Assert.True(first.Succeeded);
        Assert.False(duplicate.Succeeded);
        Assert.Contains(nameof(Account.Name), duplicate.Errors.Keys);
    }

    [Fact]
    public async Task GetBalanceSummariesAsync_calculates_opening_income_and_expense_balance()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        var service = new AccountService(context);
        AccountResult accountResult = await service.CreateAsync("現金", AccountType.Cash, 1_000m, TestDataBuilder.UsdCurrency);
        Account account = await context.Accounts.SingleAsync();
        Category income = await SeedCategoryAsync(context, "薪資", TransactionType.Income);
        Category expense = await SeedCategoryAsync(context, "餐飲", TransactionType.Expense);
        context.Transactions.AddRange(
            CreateTransaction(account.Id, income.Id, TransactionType.Income, 500m, TestDataBuilder.UsdCurrency),
            CreateTransaction(account.Id, expense.Id, TransactionType.Expense, 200m, TestDataBuilder.UsdCurrency),
            CreateTransaction(account.Id, income.Id, TransactionType.Income, 9_999m, TestDataBuilder.TwdCurrency));
        await context.SaveChangesAsync();

        var summaries = await service.GetBalanceSummariesAsync();

        Assert.True(accountResult.Succeeded);
        Assert.Equal(TestDataBuilder.UsdCurrency, summaries.Single().Currency);
        Assert.Equal(1_300m, summaries.Single().CurrentBalance);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("AUD")]
    public async Task CreateAsync_rejects_missing_or_unsupported_currency(string? currency)
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        var service = new AccountService(context);

        AccountResult result = await service.CreateAsync("外幣帳戶", AccountType.Cash, 0m, currency);

        Assert.False(result.Succeeded);
        Assert.Contains(nameof(Account.Currency), result.Errors.Keys);
        Assert.Empty(context.Accounts);
    }

    [Fact]
    public async Task UpdateAsync_rejects_currency_changes()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        var service = new AccountService(context);
        AccountResult created = await service.CreateAsync("現金", AccountType.Cash, 0m, TestDataBuilder.TwdCurrency);
        Account account = await context.Accounts.SingleAsync();

        AccountResult updated = await service.UpdateAsync(
            account.Id,
            "現金",
            AccountType.Cash,
            0m,
            TestDataBuilder.UsdCurrency,
            "wallet",
            0);

        Assert.True(created.Succeeded);
        Assert.False(updated.Succeeded);
        Assert.Contains(nameof(Account.Currency), updated.Errors.Keys);
        Assert.Equal(TestDataBuilder.TwdCurrency, (await context.Accounts.SingleAsync()).Currency);
    }

    private static Transaction CreateTransaction(long accountId, long categoryId, TransactionType type, decimal amount, string currency)
    {
        return new Transaction
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Type = type,
            Currency = currency,
            Amount = amount,
            TransactionDate = TestDataBuilder.DefaultToday,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "測試"
        };
    }

    private static async Task<Category> SeedCategoryAsync(AppDbContext context, string name, TransactionType type)
    {
        var category = new Category
        {
            Name = name,
            NormalizedName = name,
            Type = type,
            IconKey = "tag",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category;
    }
}
