using System.Globalization;
using System.Text;
using BookKeeping2.Data;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Services.Common;
using BookKeeping2.Services.Time;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping2.Services.Csv;

/// <summary>
/// CsvHelper-backed transaction CSV exporter.
/// </summary>
public sealed class CsvExportService : ICsvExportService
{
    private static readonly string[] Headers = ["日期", "類型", "金額", "分類", "帳戶", "備註"];
    private readonly AppDbContext dbContext;
    private readonly ITaipeiDateService dateService;

    /// <summary>
    /// Initializes a new CSV export service.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    /// <param name="dateService">The Taipei date service.</param>
    public CsvExportService(AppDbContext dbContext, ITaipeiDateService dateService)
    {
        this.dbContext = dbContext;
        this.dateService = dateService;
    }

    /// <inheritdoc />
    public async Task<CsvExportResult> ExportAsync(CsvExportOptions options, CancellationToken cancellationToken = default)
    {
        if (options.StartDate.HasValue && options.EndDate.HasValue && options.EndDate.Value < options.StartDate.Value)
        {
            throw new ArgumentException("結束日期不可早於起始日期。", nameof(options));
        }

        IQueryable<Transaction> query = dbContext.Transactions
            .AsNoTracking()
            .Include(transaction => transaction.Category)
            .Include(transaction => transaction.Account)
            .Where(transaction => !transaction.IsDeleted);

        if (options.StartDate.HasValue)
        {
            query = query.Where(transaction => transaction.TransactionDate >= options.StartDate.Value);
        }

        if (options.EndDate.HasValue)
        {
            query = query.Where(transaction => transaction.TransactionDate <= options.EndDate.Value);
        }

        List<Transaction> transactions = await query
            .OrderBy(transaction => transaction.TransactionDate)
            .ThenBy(transaction => transaction.Id)
            .ToListAsync(cancellationToken);

        List<CsvTransactionRow> rows = transactions.Select(ToRow).ToList();
        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            NewLine = "\r\n"
        };
        using var stringWriter = new StringWriter(CultureInfo.InvariantCulture);
        await using var csvWriter = new CsvWriter(stringWriter, configuration);

        foreach (string header in Headers)
        {
            csvWriter.WriteField(header);
        }

        await csvWriter.NextRecordAsync();
        foreach (CsvTransactionRow row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            csvWriter.WriteField(row.Date);
            csvWriter.WriteField(row.Type);
            csvWriter.WriteField(row.Amount);
            csvWriter.WriteField(row.Category);
            csvWriter.WriteField(row.Account);
            csvWriter.WriteField(row.Note);
            await csvWriter.NextRecordAsync();
        }

        string fileName = $"transactions-{dateService.NowTaipei:yyyyMMdd-HHmmss}.csv";
        return new CsvExportResult(Encoding.UTF8.GetBytes(stringWriter.ToString()), rows.Count, fileName);
    }

    private static CsvTransactionRow ToRow(Transaction transaction)
    {
        return new CsvTransactionRow
        {
            Date = transaction.TransactionDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Type = transaction.Type == TransactionType.Income ? "收入" : "支出",
            Amount = MoneyMinorUnitConverter.FromMinorUnits(transaction.AmountMinorUnits).ToString("0.##", CultureInfo.InvariantCulture),
            Category = ProtectFormulaText(transaction.Category.Name),
            Account = ProtectFormulaText(transaction.Account.Name),
            Note = ProtectFormulaText(transaction.Note)
        };
    }

    private static string ProtectFormulaText(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value[0] is '=' or '+' or '-' or '@' or '\t' or '\r' or '\n'
            ? $"'{value}"
            : value;
    }
}
