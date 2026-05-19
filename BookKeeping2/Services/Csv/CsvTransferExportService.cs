using System.Globalization;
using System.Text;
using BookKeeping2.Data;
using BookKeeping2.Services.Common;
using BookKeeping2.Services.Time;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping2.Services.Csv;

/// <summary>
/// CsvHelper-backed transfer CSV exporter.
/// </summary>
public sealed class CsvTransferExportService
{
    /// <summary>
    /// Header text for the transfer CSV export format.
    /// </summary>
    public const string HeaderText = CsvTransferImportParser.HeaderText;

    private static readonly string[] Headers = HeaderText.Split(',');
    private readonly AppDbContext dbContext;
    private readonly ITaipeiDateService dateService;

    /// <summary>
    /// Initializes a new transfer CSV export service.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    /// <param name="dateService">The Taipei date service.</param>
    public CsvTransferExportService(AppDbContext dbContext, ITaipeiDateService dateService)
    {
        this.dbContext = dbContext;
        this.dateService = dateService;
    }

    /// <summary>
    /// Exports non-deleted account transfers matching the supplied options.
    /// </summary>
    /// <param name="options">The export options.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The generated CSV content.</returns>
    public async Task<CsvExportResult> ExportAsync(CsvExportOptions options, CancellationToken cancellationToken = default)
    {
        if (options.StartDate.HasValue && options.EndDate.HasValue && options.EndDate.Value < options.StartDate.Value)
        {
            throw new ArgumentException("結束日期不可早於起始日期。", nameof(options));
        }

        var query = dbContext.AccountTransfers
            .AsNoTracking()
            .Where(transfer => !transfer.IsDeleted);

        if (options.StartDate.HasValue)
        {
            query = query.Where(transfer => transfer.TransferDate >= options.StartDate.Value);
        }

        if (options.EndDate.HasValue)
        {
            query = query.Where(transfer => transfer.TransferDate <= options.EndDate.Value);
        }

        var transfers = await query
            .OrderBy(transfer => transfer.TransferDate)
            .ThenBy(transfer => transfer.Id)
            .Select(transfer => new
            {
                transfer.TransferDate,
                transfer.Currency,
                transfer.AmountMinorUnits,
                FromAccountName = transfer.FromAccount.Name,
                ToAccountName = transfer.ToAccount.Name,
                transfer.Note
            })
            .ToListAsync(cancellationToken);

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
        foreach (var transfer in transfers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            csvWriter.WriteField(transfer.TransferDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            csvWriter.WriteField(transfer.Currency);
            csvWriter.WriteField(MoneyMinorUnitConverter.FromMinorUnits(transfer.AmountMinorUnits).ToString("0.##", CultureInfo.InvariantCulture));
            csvWriter.WriteField(ProtectFormulaText(transfer.FromAccountName));
            csvWriter.WriteField(ProtectFormulaText(transfer.ToAccountName));
            csvWriter.WriteField(ProtectFormulaText(transfer.Note));
            await csvWriter.NextRecordAsync();
        }

        string fileName = $"transfers-{dateService.NowTaipei:yyyyMMdd-HHmmss}.csv";
        return new CsvExportResult(Encoding.UTF8.GetBytes(stringWriter.ToString()), transfers.Count, fileName);
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
