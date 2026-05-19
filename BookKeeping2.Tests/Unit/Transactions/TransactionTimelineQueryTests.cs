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

namespace BookKeeping2.Tests.Unit.Transactions;

public sealed class TransactionTimelineQueryTests
{
    [Fact]
    public async Task SearchAsync_returns_income_expense_and_transfer_rows_sorted_together()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        (Account cash, Account bank, Category food, Category salary) = await SeedAsync(context);
        context.Transactions.AddRange(
            CreateTransaction(cash.Id, food.Id, TransactionType.Expense, new DateOnly(2026, 5, 1), 100m, "午餐"),
            CreateTransaction(bank.Id, salary.Id, TransactionType.Income, new DateOnly(2026, 5, 3), 1_000m, "薪水"));
        context.AccountTransfers.Add(CreateTransfer(bank.Id, cash.Id, new DateOnly(2026, 5, 2), 500m, "提款"));
        await context.SaveChangesAsync();
        var service = new TransactionQueryService(context);

        var result = await service.SearchAsync(new TransactionQuery { PageSize = 10 });

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(["Income", "Transfer", "Expense"], result.Items.Select(item => item.RecordKind));
        var transfer = Assert.Single(result.Items, item => item.RecordKind == "Transfer");
        Assert.Equal("轉帳", transfer.TypeText);
        Assert.Equal("銀行 -> 現金", transfer.TransferDirectionText);
        Assert.Equal("/Transfers/Edit", transfer.EditPage);
        Assert.Equal("/Transfers/Delete", transfer.DeletePage);
        Assert.Equal(string.Empty, transfer.CategoryName);
    }

    [Fact]
    public async Task SearchAsync_filters_transfer_rows_by_account_currency_keyword_amount_and_excludes_them_for_category_filter()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        (Account cash, Account bank, Category food, _) = await SeedAsync(context);
        Account wallet = await SeedAccountAsync(context, "電子錢包");
        context.Transactions.Add(CreateTransaction(cash.Id, food.Id, TransactionType.Expense, TestDataBuilder.DefaultToday, 100m, "午餐"));
        context.AccountTransfers.AddRange(
            CreateTransfer(bank.Id, cash.Id, TestDataBuilder.DefaultToday, 500m, "提款轉現金"),
            CreateTransfer(wallet.Id, cash.Id, TestDataBuilder.DefaultToday, 50m, "小額轉帳"),
            CreateTransfer(bank.Id, wallet.Id, TestDataBuilder.DefaultToday, 500m, "已刪除", isDeleted: true));
        await context.SaveChangesAsync();
        var service = new TransactionQueryService(context);

        var filtered = await service.SearchAsync(new TransactionQuery
        {
            AccountId = bank.Id,
            Currency = TestDataBuilder.TwdCurrency,
            Keyword = "提款",
            MinAmount = 400m,
            MaxAmount = 600m,
            PageSize = 10
        });
        var categoryFiltered = await service.SearchAsync(new TransactionQuery { CategoryId = food.Id, PageSize = 10 });

        var transfer = Assert.Single(filtered.Items);
        Assert.Equal("Transfer", transfer.RecordKind);
        Assert.Equal("提款轉現金", transfer.Note);
        Assert.Single(categoryFiltered.Items);
        Assert.All(categoryFiltered.Items, item => Assert.NotEqual("Transfer", item.RecordKind));
    }

    private static Transaction CreateTransaction(
        long accountId,
        long categoryId,
        TransactionType type,
        DateOnly date,
        decimal amount,
        string note)
    {
        return new Transaction
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Type = type,
            TransactionDate = date,
            Currency = TestDataBuilder.TwdCurrency,
            Amount = amount,
            Note = note,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "測試"
        };
    }

    private static AccountTransfer CreateTransfer(
        long fromAccountId,
        long toAccountId,
        DateOnly date,
        decimal amount,
        string note,
        bool isDeleted = false)
    {
        return new AccountTransfer
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            TransferDate = date,
            Currency = TestDataBuilder.TwdCurrency,
            Amount = amount,
            Note = note,
            SubmissionToken = Guid.NewGuid().ToString("N"),
            IsDeleted = isDeleted,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "測試"
        };
    }

    private static async Task<(Account Cash, Account Bank, Category Food, Category Salary)> SeedAsync(AppDbContext context)
    {
        Account cash = await SeedAccountAsync(context, "現金");
        Account bank = await SeedAccountAsync(context, "銀行");
        var food = new Category { Name = "餐飲", NormalizedName = "餐飲", Type = TransactionType.Expense, IconKey = "tag", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var salary = new Category { Name = "薪資", NormalizedName = "薪資", Type = TransactionType.Income, IconKey = "tag", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        context.Categories.AddRange(food, salary);
        await context.SaveChangesAsync();
        return (cash, bank, food, salary);
    }

    private static async Task<Account> SeedAccountAsync(AppDbContext context, string name)
    {
        Account account = TestDataBuilder.CreateAccount(name, TestDataBuilder.TwdCurrency);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        return account;
    }
}
