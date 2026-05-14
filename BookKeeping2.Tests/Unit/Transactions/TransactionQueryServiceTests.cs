using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Services.Transactions;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookKeeping2.Tests.Unit.Transactions;

public sealed class TransactionQueryServiceTests
{
    [Fact]
    public async Task SearchAsync_applies_and_logic_for_category_account_date_amount_and_keyword()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        (Account cash, Account bank, Category food, Category transport) = await SeedAsync(context);
        context.Transactions.AddRange(
            Create(cash.Id, food.Id, new DateOnly(2026, 2, 10), 150m, "午餐便當", TestDataBuilder.TwdCurrency),
            Create(cash.Id, food.Id, new DateOnly(2026, 2, 11), 500m, "晚餐", TestDataBuilder.TwdCurrency),
            Create(bank.Id, food.Id, new DateOnly(2026, 2, 10), 150m, "午餐便當", TestDataBuilder.TwdCurrency),
            Create(cash.Id, transport.Id, new DateOnly(2026, 2, 10), 150m, "午餐便當", TestDataBuilder.TwdCurrency),
            Create(cash.Id, food.Id, new DateOnly(2026, 1, 31), 150m, "午餐便當", TestDataBuilder.TwdCurrency),
            Create(cash.Id, food.Id, new DateOnly(2026, 2, 10), 150m, "午餐便當", TestDataBuilder.UsdCurrency));
        await context.SaveChangesAsync();
        var service = new TransactionQueryService(context);

        var result = await service.SearchAsync(new TransactionQuery
        {
            StartDate = new DateOnly(2026, 2, 1),
            EndDate = new DateOnly(2026, 2, 28),
            CategoryId = food.Id,
            AccountId = cash.Id,
            Currency = TestDataBuilder.TwdCurrency,
            MinAmount = 100m,
            MaxAmount = 200m,
            Keyword = "便當",
            Page = 1,
            PageSize = 20
        });

        var item = Assert.Single(result.Items);
        Assert.Equal("午餐便當", item.Note);
        Assert.Equal(TestDataBuilder.TwdCurrency, item.Currency);
        Assert.Equal(1, result.TotalCount);
    }

    private static Transaction Create(long accountId, long categoryId, DateOnly date, decimal amount, string note, string currency)
    {
        return new Transaction
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Type = TransactionType.Expense,
            TransactionDate = date,
            Currency = currency,
            Amount = amount,
            Note = note,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "測試"
        };
    }

    private static async Task<(Account Cash, Account Bank, Category Food, Category Transport)> SeedAsync(AppDbContext context)
    {
        var cash = new Account { Name = "現金", NormalizedName = "現金", Type = AccountType.Cash, IconKey = "wallet", Currency = "TWD", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var bank = new Account { Name = "銀行", NormalizedName = "銀行", Type = AccountType.Bank, IconKey = "bank", Currency = "TWD", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var food = new Category { Name = "餐飲", NormalizedName = "餐飲", Type = TransactionType.Expense, IconKey = "tag", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var transport = new Category { Name = "交通", NormalizedName = "交通", Type = TransactionType.Expense, IconKey = "tag", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        context.AddRange(cash, bank, food, transport);
        await context.SaveChangesAsync();
        return (cash, bank, food, transport);
    }
}
