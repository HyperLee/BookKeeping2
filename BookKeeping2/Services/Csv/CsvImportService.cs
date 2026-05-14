using System.Globalization;
using BookKeeping2.Data;
using BookKeeping2.Data.SeedData;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Audit;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.CsvImports;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Services.Audit;
using BookKeeping2.Services.Common;
using BookKeeping2.Services.Time;
using BookKeeping2.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookKeeping2.Services.Csv;

/// <summary>
/// Imports valid CSV rows as transactions and persists row-level errors.
/// </summary>
public sealed class CsvImportService : ICsvImportService
{
    private readonly AppDbContext dbContext;
    private readonly ITaipeiDateService dateService;
    private readonly IAuditService auditService;
    private readonly CsvImportParser parser;
    private readonly TextInputSanitizer sanitizer;

    /// <summary>
    /// Initializes a new CSV import service.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    /// <param name="dateService">The Taipei date service.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="parser">The optional CSV parser.</param>
    /// <param name="sanitizer">The optional text sanitizer.</param>
    public CsvImportService(
        AppDbContext dbContext,
        ITaipeiDateService dateService,
        IAuditService auditService,
        CsvImportParser? parser = null,
        TextInputSanitizer? sanitizer = null)
    {
        this.dbContext = dbContext;
        this.dateService = dateService;
        this.auditService = auditService;
        this.parser = parser ?? new CsvImportParser();
        this.sanitizer = sanitizer ?? new TextInputSanitizer();
    }

    /// <inheritdoc />
    public async Task<CsvImportResult> ImportAsync(CsvImportCommand command, CancellationToken cancellationToken = default)
    {
        CsvImportResult result = parser.Parse(command);
        if (result.Rows.Count == 0)
        {
            await PersistBatchAsync(result, cancellationToken);
            return result;
        }

        List<Account> accounts = await dbContext.Accounts
            .Where(account => !account.IsArchived)
            .ToListAsync(cancellationToken);
        List<Category> categories = await dbContext.Categories
            .Where(category => !category.IsArchived)
            .ToListAsync(cancellationToken);
        Dictionary<string, Account> accountByName = accounts.ToDictionary(account => account.NormalizedName);
        Dictionary<string, Category> categoryByTypeAndName = categories.ToDictionary(CategoryKey);
        List<Transaction> transactions = [];

        foreach (CsvImportRow row in result.Rows)
        {
            ValidatedImportRow? validated = ValidateRow(row, result, accountByName);
            if (validated is null)
            {
                continue;
            }

            string categoryKey = CategoryKey(validated.Type, validated.NormalizedCategoryName);
            if (!categoryByTypeAndName.TryGetValue(categoryKey, out Category? category))
            {
                category = new Category
                {
                    Name = validated.CategoryName,
                    NormalizedName = validated.NormalizedCategoryName,
                    Type = validated.Type,
                    IconKey = validated.Type == TransactionType.Expense ? "tag" : "cash-coin",
                    CreatedAtUtc = dateService.UtcNow,
                    UpdatedAtUtc = dateService.UtcNow
                };
                categoryByTypeAndName[categoryKey] = category;
                result.CreatedCategories.Add(category.Name);
                dbContext.Categories.Add(category);
            }

            transactions.Add(new Transaction
            {
                TransactionDate = validated.TransactionDate,
                Type = validated.Type,
                AmountMinorUnits = validated.AmountMinorUnits,
                Currency = validated.Currency,
                Category = category,
                AccountId = validated.Account.Id,
                Note = sanitizer.SanitizePlainText(row.Note),
                CreatedAtUtc = dateService.UtcNow,
                UpdatedAtUtc = dateService.UtcNow,
                LastChangeSummary = $"CSV 匯入 {validated.Currency}"
            });
        }

        result.SucceededRows = transactions.Count;
        result.FailedRows = result.Errors.Count;
        result.Summary = CsvImportResultFormatter.Format(result);

        await using IDbContextTransaction transactionScope = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        dbContext.Transactions.AddRange(transactions);
        await PersistBatchAsync(result, cancellationToken);
        await auditService.RecordAsync(
            AuditEventType.CsvImported,
            nameof(CsvImportBatch),
            null,
            result.Summary,
            cancellationToken: cancellationToken);
        await transactionScope.CommitAsync(cancellationToken);

        return result;
    }

    private ValidatedImportRow? ValidateRow(CsvImportRow row, CsvImportResult result, IReadOnlyDictionary<string, Account> accountByName)
    {
        if (!DateOnly.TryParseExact(row.Date.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly transactionDate))
        {
            result.AddError(row.RowNumber, "日期", "日期無效", Preview(row.Date));
            return null;
        }

        if (transactionDate > dateService.Today)
        {
            result.AddError(row.RowNumber, "日期", FinancialValidationMessages.DateCannotBeFuture, Preview(row.Date));
            return null;
        }

        TransactionType type;
        string typeText = row.Type.Trim();
        if (typeText == "收入")
        {
            type = TransactionType.Income;
        }
        else if (typeText == "支出")
        {
            type = TransactionType.Expense;
        }
        else
        {
            result.AddError(row.RowNumber, "類型", "類型必須為收入或支出", Preview(row.Type));
            return null;
        }

        if (!SupportedCurrency.TryNormalize(row.Currency, out string? normalizedCurrency))
        {
            result.AddError(row.RowNumber, "幣別", "幣別不支援，請使用 TWD、USD、JPY、EUR 或 GBP", Preview(row.Currency));
            return null;
        }

        string currencyCode = normalizedCurrency!;

        if (!decimal.TryParse(row.Amount.Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal amount))
        {
            result.AddError(row.RowNumber, "金額", "金額格式無效", Preview(row.Amount));
            return null;
        }

        long amountMinorUnits;
        try
        {
            amountMinorUnits = MoneyMinorUnitConverter.ToMinorUnits(amount);
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException or OverflowException)
        {
            result.AddError(row.RowNumber, "金額", exception.Message, Preview(row.Amount));
            return null;
        }

        string categoryName = row.Category.Trim();
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            result.AddError(row.RowNumber, "分類", "分類不可空白", Preview(row.Category));
            return null;
        }

        string accountName = row.Account.Trim();
        string normalizedAccountName = DefaultSeedData.NormalizeName(accountName);
        if (!accountByName.TryGetValue(normalizedAccountName, out Account? account))
        {
            result.AddError(row.RowNumber, "帳戶", "帳戶不存在", Preview(row.Account));
            return null;
        }

        if (!string.Equals(account.Currency, currencyCode, StringComparison.Ordinal))
        {
            result.AddError(row.RowNumber, "帳戶", "帳戶幣別與交易幣別不一致", Preview(row.Account));
            return null;
        }

        return new ValidatedImportRow(
            transactionDate,
            type,
            amountMinorUnits,
            currencyCode,
            categoryName,
            DefaultSeedData.NormalizeName(categoryName),
            account);
    }

    private async Task PersistBatchAsync(CsvImportResult result, CancellationToken cancellationToken)
    {
        result.FailedRows = result.Errors.Count;
        result.Summary = CsvImportResultFormatter.Format(result);
        var batch = new CsvImportBatch
        {
            OriginalFileName = result.FileName,
            ImportedAtUtc = dateService.UtcNow,
            TotalRows = result.TotalRows,
            SucceededRows = result.SucceededRows,
            FailedRows = result.FailedRows,
            CreatedCategoryCount = result.CreatedCategories.Count,
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

    private static string CategoryKey(Category category)
    {
        return CategoryKey(category.Type, category.NormalizedName);
    }

    private static string CategoryKey(TransactionType type, string normalizedName)
    {
        return $"{(int)type}:{normalizedName}";
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

    private sealed record ValidatedImportRow(
        DateOnly TransactionDate,
        TransactionType Type,
        long AmountMinorUnits,
        string Currency,
        string CategoryName,
        string NormalizedCategoryName,
        Account Account);
}
