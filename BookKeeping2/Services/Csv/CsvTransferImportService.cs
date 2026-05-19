using System.Globalization;
using BookKeeping2.Data;
using BookKeeping2.Data.SeedData;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.AccountTransfers;
using BookKeeping2.Models.Audit;
using BookKeeping2.Models.CsvImports;
using BookKeeping2.Models.Common;
using BookKeeping2.Services.AccountTransfers;
using BookKeeping2.Services.Audit;
using BookKeeping2.Services.Common;
using BookKeeping2.Services.Time;
using BookKeeping2.Validation;
using BookKeeping2.ViewModels.AccountTransfers;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping2.Services.Csv;

/// <summary>
/// Imports valid transfer CSV rows as account transfers and persists row-level errors.
/// </summary>
public sealed class CsvTransferImportService
{
    private readonly AppDbContext dbContext;
    private readonly ITaipeiDateService dateService;
    private readonly IAuditService auditService;
    private readonly IAccountTransferService transferService;
    private readonly CsvTransferImportParser parser;

    /// <summary>
    /// Initializes a new transfer CSV import service.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    /// <param name="dateService">The Taipei date service.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="transferService">The account transfer service.</param>
    /// <param name="parser">The optional transfer CSV parser.</param>
    public CsvTransferImportService(
        AppDbContext dbContext,
        ITaipeiDateService dateService,
        IAuditService auditService,
        IAccountTransferService transferService,
        CsvTransferImportParser? parser = null)
    {
        this.dbContext = dbContext;
        this.dateService = dateService;
        this.auditService = auditService;
        this.transferService = transferService;
        this.parser = parser ?? new CsvTransferImportParser();
    }

    /// <summary>
    /// Imports a transfer CSV file.
    /// </summary>
    /// <param name="command">The import command.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The import result.</returns>
    public async Task<CsvTransferImportResult> ImportAsync(CsvImportCommand command, CancellationToken cancellationToken = default)
    {
        CsvTransferImportResult result = parser.Parse(command);
        if (result.Rows.Count > 0)
        {
            List<Account> accounts = await dbContext.Accounts
                .AsNoTracking()
                .Where(account => !account.IsArchived)
                .ToListAsync(cancellationToken);
            Dictionary<string, Account> accountByName = accounts.ToDictionary(account => account.NormalizedName);

            foreach (CsvTransferRow row in result.Rows)
            {
                ValidatedTransferRow? validated = ValidateRow(row, result, accountByName);
                if (validated is null)
                {
                    continue;
                }

                AccountTransferResult transferResult = await transferService.CreateAsync(new AccountTransferInputModel
                {
                    TransferDate = validated.TransferDate,
                    Currency = validated.Currency,
                    Amount = validated.Amount,
                    FromAccountId = validated.FromAccount.Id,
                    ToAccountId = validated.ToAccount.Id,
                    Note = row.Note,
                    SubmissionToken = Guid.NewGuid().ToString("N")
                }, cancellationToken);

                if (transferResult.Succeeded)
                {
                    result.SucceededRows++;
                }
                else
                {
                    foreach (string message in transferResult.Errors.SelectMany(error => error.Value))
                    {
                        result.AddError(row.RowNumber, "轉帳", message);
                    }
                }
            }
        }

        result.FailedRows = result.Errors.Count;
        result.Summary = CsvTransferImportParser.FormatSummary(result);
        await PersistBatchAsync(result, cancellationToken);
        await auditService.RecordAsync(
            AuditEventType.TransferCsvImported,
            nameof(CsvImportBatch),
            null,
            result.Summary,
            result.FailedRows > 0 ? "Warning" : "Information",
            cancellationToken: cancellationToken);

        return result;
    }

    private ValidatedTransferRow? ValidateRow(
        CsvTransferRow row,
        CsvTransferImportResult result,
        IReadOnlyDictionary<string, Account> accountByName)
    {
        if (!DateOnly.TryParseExact(row.Date.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly transferDate))
        {
            result.AddError(row.RowNumber, "日期", "日期無效", Preview(row.Date));
            return null;
        }

        if (transferDate > dateService.Today)
        {
            result.AddError(row.RowNumber, "日期", FinancialValidationMessages.DateCannotBeFuture, Preview(row.Date));
            return null;
        }

        if (!SupportedCurrency.TryNormalize(row.Currency, out string? normalizedCurrency))
        {
            result.AddError(row.RowNumber, "幣別", "幣別不支援，請使用 TWD、USD、JPY、EUR 或 GBP", Preview(row.Currency));
            return null;
        }

        if (!decimal.TryParse(row.Amount.Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal amount))
        {
            result.AddError(row.RowNumber, "金額", "金額格式無效", Preview(row.Amount));
            return null;
        }

        try
        {
            _ = MoneyMinorUnitConverter.ToMinorUnits(amount);
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException or OverflowException)
        {
            result.AddError(row.RowNumber, "金額", exception.Message, Preview(row.Amount));
            return null;
        }

        if (!accountByName.TryGetValue(DefaultSeedData.NormalizeName(row.FromAccount.Trim()), out Account? fromAccount))
        {
            result.AddError(row.RowNumber, "轉出帳戶", "帳戶不存在", Preview(row.FromAccount));
            return null;
        }

        if (!accountByName.TryGetValue(DefaultSeedData.NormalizeName(row.ToAccount.Trim()), out Account? toAccount))
        {
            result.AddError(row.RowNumber, "轉入帳戶", "帳戶不存在", Preview(row.ToAccount));
            return null;
        }

        if (fromAccount.Id == toAccount.Id)
        {
            result.AddError(row.RowNumber, "轉入帳戶", "轉出帳戶與轉入帳戶不可相同。");
            return null;
        }

        if (fromAccount.Currency != toAccount.Currency || fromAccount.Currency != normalizedCurrency)
        {
            result.AddError(row.RowNumber, "幣別", "轉出帳戶、轉入帳戶與轉帳幣別必須一致。");
            return null;
        }

        return new ValidatedTransferRow(transferDate, normalizedCurrency!, amount, fromAccount, toAccount);
    }

    private async Task PersistBatchAsync(CsvTransferImportResult result, CancellationToken cancellationToken)
    {
        var batch = new CsvImportBatch
        {
            OriginalFileName = result.FileName,
            ImportedAtUtc = dateService.UtcNow,
            TotalRows = result.TotalRows,
            SucceededRows = result.SucceededRows,
            FailedRows = result.FailedRows,
            Summary = result.Summary
        };

        foreach (CsvImportErrorDetail error in result.Errors)
        {
            batch.Errors.Add(new CsvImportError
            {
                RowNumber = error.RowNumber,
                FieldName = error.FieldName,
                Reason = error.Reason,
                RawValuePreview = error.RawValuePreview
            });
        }

        dbContext.CsvImportBatches.Add(batch);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string? Preview(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length <= 50 ? trimmed : trimmed[..50];
    }

    private sealed record ValidatedTransferRow(
        DateOnly TransferDate,
        string Currency,
        decimal Amount,
        Account FromAccount,
        Account ToAccount);
}
