using System.Text;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Services.AccountTransfers;
using BookKeeping2.Services.Audit;
using BookKeeping2.Services.Csv;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BookKeeping2.Tests.Unit.Csv;

public sealed class CsvTransferImportServiceTests
{
    [Fact]
    public async Task ImportAsync_creates_valid_rows_skips_invalid_rows_and_persists_batch_errors()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        await SeedAccountAsync(context, "銀行", TestDataBuilder.TwdCurrency);
        await SeedAccountAsync(context, "現金", TestDataBuilder.TwdCurrency);
        await SeedAccountAsync(context, "美元現金", TestDataBuilder.UsdCurrency);
        CsvTransferImportService service = CreateService(context);
        string csv = "日期,幣別,金額,轉出帳戶,轉入帳戶,備註\r\n"
            + "2026-05-01,TWD,1000,銀行,現金,有效提款\r\n"
            + "2026-05-02,TWD,1000,銀行,美元現金,跨幣別\r\n"
            + "2026-05-03,TWD,1000,不存在,現金,帳戶不存在";

        CsvTransferImportResult result = await service.ImportAsync(new CsvImportCommand("transfers.csv", Encoding.UTF8.GetBytes(csv)));

        Assert.Equal(3, result.TotalRows);
        Assert.Equal(1, result.SucceededRows);
        Assert.Equal(2, result.FailedRows);
        Assert.Equal(1, await context.AccountTransfers.CountAsync());
        Assert.Equal(2, await context.CsvImportErrors.CountAsync());
        Assert.Contains(await context.AuditEvents.ToListAsync(), audit => audit.EventType.ToString() == "TransferCsvImported");
    }

    private static CsvTransferImportService CreateService(AppDbContext context)
    {
        FakeTaipeiDateService dateService = TestDataBuilder.CreateDateService();
        var auditService = new AuditService(context, dateService, NullLogger<AuditService>.Instance);
        var transferService = new AccountTransferService(context, dateService, auditService, new AuditLogMaskingPolicy());
        return new CsvTransferImportService(context, dateService, auditService, transferService);
    }

    private static async Task SeedAccountAsync(AppDbContext context, string name, string currency)
    {
        Account account = TestDataBuilder.CreateAccount(name, currency);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
    }
}
