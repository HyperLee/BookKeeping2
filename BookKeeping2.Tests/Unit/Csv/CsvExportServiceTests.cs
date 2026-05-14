using System.Diagnostics;
using System.Text;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Services.Csv;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookKeeping2.Tests.Unit.Csv;

public sealed class CsvExportServiceTests
{
    [Fact]
    public async Task ExportAsync_writes_fixed_columns_date_range_and_excludes_deleted_transactions()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        (Account account, Category food) = await SeedAsync(context);
        context.Transactions.AddRange(
            Create(account.Id, food.Id, new DateOnly(2026, 1, 31), 99m, "一月"),
            Create(account.Id, food.Id, new DateOnly(2026, 2, 1), 150m, "二月一日"),
            Create(account.Id, food.Id, new DateOnly(2026, 2, 28), 200.5m, "二月月底"),
            Create(account.Id, food.Id, new DateOnly(2026, 2, 10), 999m, "已刪除", isDeleted: true));
        await context.SaveChangesAsync();
        var service = new CsvExportService(context, TestDataBuilder.CreateDateService());

        CsvExportResult result = await service.ExportAsync(new CsvExportOptions
        {
            StartDate = new DateOnly(2026, 2, 1),
            EndDate = new DateOnly(2026, 2, 28)
        });
        string csv = Encoding.UTF8.GetString(result.Content);

        Assert.StartsWith("日期,類型,幣別,金額,分類,帳戶,備註\r\n", csv, StringComparison.Ordinal);
        Assert.Contains("2026-02-01,支出,TWD,150,餐飲,現金,二月一日", csv);
        Assert.Contains("2026-02-28,支出,TWD,200.5,餐飲,現金,二月月底", csv);
        Assert.DoesNotContain("一月", csv);
        Assert.DoesNotContain("已刪除", csv);
        Assert.Equal(2, result.RowCount);
    }

    [Fact]
    public async Task ExportAsync_writes_original_currency_and_amount_without_conversion()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        (_, Category food) = await SeedAsync(context);
        var usdAccount = new Account { Name = "美元現金", NormalizedName = "美元現金", Type = AccountType.Cash, IconKey = "wallet", Currency = TestDataBuilder.UsdCurrency, CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        context.Accounts.Add(usdAccount);
        await context.SaveChangesAsync();
        context.Transactions.Add(Create(usdAccount.Id, food.Id, new DateOnly(2026, 2, 10), 123.45m, "美元", currency: TestDataBuilder.UsdCurrency));
        await context.SaveChangesAsync();
        var service = new CsvExportService(context, TestDataBuilder.CreateDateService());

        CsvExportResult result = await service.ExportAsync(new CsvExportOptions());
        string csv = Encoding.UTF8.GetString(result.Content);

        Assert.Contains("2026-02-10,支出,USD,123.45,餐飲,美元現金,美元", csv);
        Assert.Equal(1, result.RowCount);
    }

    [Fact]
    public async Task ExportAsync_exports_1000_transactions_under_five_seconds()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        (Account account, Category food) = await SeedAsync(context);
        for (int i = 0; i < 1_000; i++)
        {
            context.Transactions.Add(Create(account.Id, food.Id, new DateOnly(2026, 3, (i % 28) + 1), 10m, $"測試 {i}"));
        }
        await context.SaveChangesAsync();
        var service = new CsvExportService(context, TestDataBuilder.CreateDateService());
        Stopwatch stopwatch = Stopwatch.StartNew();

        CsvExportResult result = await service.ExportAsync(new CsvExportOptions());

        stopwatch.Stop();
        Assert.Equal(1_000, result.RowCount);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5));
    }

    private static Transaction Create(long accountId, long categoryId, DateOnly date, decimal amount, string note, bool isDeleted = false, string currency = TestDataBuilder.TwdCurrency)
    {
        return new Transaction
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Type = TransactionType.Expense,
            TransactionDate = date,
            Amount = amount,
            Currency = currency,
            Note = note,
            IsDeleted = isDeleted,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "測試"
        };
    }

    private static async Task<(Account Account, Category Food)> SeedAsync(AppDbContext context)
    {
        var account = new Account { Name = "現金", NormalizedName = "現金", Type = AccountType.Cash, IconKey = "wallet", Currency = "TWD", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var food = new Category { Name = "餐飲", NormalizedName = "餐飲", Type = TransactionType.Expense, IconKey = "tag", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        context.AddRange(account, food);
        await context.SaveChangesAsync();
        return (account, food);
    }
}
