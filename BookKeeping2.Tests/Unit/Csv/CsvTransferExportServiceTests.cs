using System.Text;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.AccountTransfers;
using BookKeeping2.Services.Csv;
using BookKeeping2.Tests.TestSupport;
using Xunit;

namespace BookKeeping2.Tests.Unit.Csv;

public sealed class CsvTransferExportServiceTests
{
    [Fact]
    public async Task ExportAsync_writes_exact_header_filters_deleted_rows_orders_by_date_and_protects_formulas()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        Account bank = TestDataBuilder.CreateAccount("=銀行", TestDataBuilder.TwdCurrency);
        Account cash = TestDataBuilder.CreateAccount("現金", TestDataBuilder.TwdCurrency);
        context.Accounts.AddRange(bank, cash);
        await context.SaveChangesAsync();
        context.AccountTransfers.AddRange(
            CreateTransfer(bank.Id, cash.Id, new DateOnly(2026, 5, 2), 200m, "+note"),
            CreateTransfer(bank.Id, cash.Id, new DateOnly(2026, 5, 1), 100m, "first"),
            CreateTransfer(bank.Id, cash.Id, new DateOnly(2026, 5, 3), 300m, "deleted", isDeleted: true));
        await context.SaveChangesAsync();
        var service = new CsvTransferExportService(context, TestDataBuilder.CreateDateService());

        CsvExportResult result = await service.ExportAsync(new CsvExportOptions());

        string csv = Encoding.UTF8.GetString(result.Content);
        string[] lines = csv.Trim().Split("\r\n");
        Assert.Equal("日期,幣別,金額,轉出帳戶,轉入帳戶,備註", lines[0]);
        Assert.Contains("2026-05-01", lines[1]);
        Assert.Contains("2026-05-02", lines[2]);
        Assert.DoesNotContain("deleted", csv);
        Assert.Contains("'=銀行", csv);
        Assert.Contains("'+note", csv);
        Assert.Equal(2, result.RowCount);
        Assert.StartsWith("transfers-", result.FileName, StringComparison.Ordinal);
    }

    private static AccountTransfer CreateTransfer(long fromAccountId, long toAccountId, DateOnly date, decimal amount, string note, bool isDeleted = false)
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
}
