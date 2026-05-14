using System.Diagnostics;
using System.Text;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Audit;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Services.Audit;
using BookKeeping2.Services.Csv;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BookKeeping2.Tests.Unit.Csv;

public sealed class CsvImportServiceTests
{
    [Fact]
    public async Task ImportAsync_validates_rows_creates_missing_categories_persists_partial_success_and_finishes_100_rows_under_ten_seconds()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        await SeedAsync(context);
        string csv = "日期,類型,金額,分類,帳戶,備註\r\n"
            + "2026-02-01,支出,150,餐飲,現金,午餐\r\n"
            + "2026-02-02,收入,200,利息,現金,自動分類\r\n"
            + "2026-02-03,支出,100,餐飲,不存在,錯誤\r\n"
            + string.Join("\r\n", Enumerable.Range(1, 100).Select(i => $"2026-03-{((i - 1) % 28) + 1:00},支出,10,餐飲,現金,批次 {i}"));
        var service = CreateService(context);
        Stopwatch stopwatch = Stopwatch.StartNew();

        CsvImportResult result = await service.ImportAsync(new CsvImportCommand("import.csv", Encoding.UTF8.GetBytes(csv)));

        stopwatch.Stop();
        Assert.Equal(102, result.SucceededRows);
        Assert.Equal(1, result.FailedRows);
        Assert.Contains("利息", result.CreatedCategories);
        Assert.Contains(result.Errors, error => error.RowNumber == 4 && error.Reason.Contains("帳戶不存在", StringComparison.Ordinal));
        Assert.Equal(102, await context.Transactions.CountAsync());
        Assert.True(await context.Categories.AnyAsync(category => category.Name == "利息" && category.Type == TransactionType.Income));
        Assert.True(await context.CsvImportBatches.AnyAsync(batch => batch.SucceededRows == 102 && batch.FailedRows == 1));
        Assert.True(await context.AuditEvents.AnyAsync(audit => audit.EventType == AuditEventType.CsvImported));
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task ImportAsync_handles_supported_currency_legacy_default_and_currency_row_errors()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        await SeedAsync(context);
        var service = CreateService(context);
        string sevenColumnCsv = "日期,類型,幣別,金額,分類,帳戶,備註\r\n"
            + "2026-02-01,支出,eur,123.45,餐飲,歐元現金,歐元午餐\r\n"
            + "2026-02-02,支出,AUD,100,餐飲,歐元現金,不支援幣別\r\n"
            + "2026-02-03,支出, ,100,餐飲,歐元現金,空白幣別\r\n"
            + "2026-02-04,支出,USD,100,餐飲,現金,帳戶幣別不一致";
        string legacyCsv = "日期,類型,金額,分類,帳戶,備註\r\n"
            + "2026-02-05,支出,77,餐飲,現金,legacy";

        CsvImportResult sevenColumnResult = await service.ImportAsync(new CsvImportCommand("currency.csv", Encoding.UTF8.GetBytes(sevenColumnCsv)));
        CsvImportResult legacyResult = await service.ImportAsync(new CsvImportCommand("legacy.csv", Encoding.UTF8.GetBytes(legacyCsv)));

        Assert.Equal(1, sevenColumnResult.SucceededRows);
        Assert.Equal(3, sevenColumnResult.FailedRows);
        Assert.Contains(sevenColumnResult.Errors, error => error.FieldName == "幣別" && error.Reason.Contains("幣別不支援", StringComparison.Ordinal));
        Assert.Contains(sevenColumnResult.Errors, error => error.FieldName == "幣別" && error.Reason.Contains("幣別不可空白", StringComparison.Ordinal));
        Assert.Contains(sevenColumnResult.Errors, error => error.FieldName == "帳戶" && error.Reason.Contains("帳戶幣別與交易幣別不一致", StringComparison.Ordinal));
        Assert.Equal(1, legacyResult.SucceededRows);
        Assert.Equal(0, legacyResult.FailedRows);

        var imported = await context.Transactions
            .OrderBy(transaction => transaction.TransactionDate)
            .Select(transaction => new { transaction.Currency, transaction.Amount, transaction.Note })
            .ToListAsync();
        Assert.Contains(imported, transaction => transaction.Currency == TestDataBuilder.EurCurrency && transaction.Amount == 123.45m && transaction.Note == "歐元午餐");
        Assert.Contains(imported, transaction => transaction.Currency == TestDataBuilder.TwdCurrency && transaction.Amount == 77m && transaction.Note == "legacy");
    }

    private static CsvImportService CreateService(AppDbContext context)
    {
        FakeTaipeiDateService dateService = TestDataBuilder.CreateDateService();
        var auditService = new AuditService(context, dateService, NullLogger<AuditService>.Instance);
        return new CsvImportService(context, dateService, auditService);
    }

    private static async Task SeedAsync(AppDbContext context)
    {
        var account = new Account { Name = "現金", NormalizedName = "現金", Type = AccountType.Cash, IconKey = "wallet", Currency = "TWD", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var eurAccount = new Account { Name = "歐元現金", NormalizedName = "歐元現金", Type = AccountType.Cash, IconKey = "wallet", Currency = TestDataBuilder.EurCurrency, CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var food = new Category { Name = "餐飲", NormalizedName = "餐飲", Type = TransactionType.Expense, IconKey = "tag", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        context.AddRange(account, eurAccount, food);
        await context.SaveChangesAsync();
    }
}
