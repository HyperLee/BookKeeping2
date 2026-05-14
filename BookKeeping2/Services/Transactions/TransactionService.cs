using BookKeeping2.Data;
using BookKeeping2.Models.Audit;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Services.Audit;
using BookKeeping2.Services.Budgets;
using BookKeeping2.Services.Common;
using BookKeeping2.Services.Time;
using BookKeeping2.Validation;
using BookKeeping2.ViewModels.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookKeeping2.Services.Transactions;

/// <summary>
/// EF Core implementation of transaction workflows.
/// </summary>
public sealed class TransactionService : ITransactionService
{
    private readonly AppDbContext dbContext;
    private readonly ITaipeiDateService dateService;
    private readonly IAuditService auditService;
    private readonly AuditLogMaskingPolicy maskingPolicy;
    private readonly TextInputSanitizer sanitizer;
    private readonly TransactionFormOptionsService formOptionsService;
    private readonly IBudgetService? budgetService;

    /// <summary>
    /// Initializes a new transaction service.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    /// <param name="dateService">The Taipei date service.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="maskingPolicy">The audit masking policy.</param>
    /// <param name="sanitizer">The text sanitizer.</param>
    /// <param name="formOptionsService">The form options service.</param>
    /// <param name="budgetService">The optional budget service.</param>
    public TransactionService(
        AppDbContext dbContext,
        ITaipeiDateService dateService,
        IAuditService auditService,
        AuditLogMaskingPolicy maskingPolicy,
        TextInputSanitizer? sanitizer = null,
        TransactionFormOptionsService? formOptionsService = null,
        IBudgetService? budgetService = null)
    {
        this.dbContext = dbContext;
        this.dateService = dateService;
        this.auditService = auditService;
        this.maskingPolicy = maskingPolicy;
        this.sanitizer = sanitizer ?? new TextInputSanitizer();
        this.formOptionsService = formOptionsService ?? new TransactionFormOptionsService(dbContext);
        this.budgetService = budgetService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TransactionListItemViewModel>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Transactions
            .AsNoTracking()
            .Include(transaction => transaction.Category)
            .Include(transaction => transaction.Account)
            .Where(transaction => !transaction.IsDeleted)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Id)
            .Select(transaction => ToListItem(transaction))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TransactionListItemViewModel?> GetDetailsAsync(long id, CancellationToken cancellationToken = default)
    {
        Transaction? transaction = await dbContext.Transactions
            .AsNoTracking()
            .Include(item => item.Category)
            .Include(item => item.Account)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

        return transaction is null ? null : ToListItem(transaction);
    }

    /// <inheritdoc />
    public async Task<TransactionInputModel?> GetForEditAsync(long id, CancellationToken cancellationToken = default)
    {
        Transaction? transaction = await dbContext.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

        if (transaction is null)
        {
            return null;
        }

        return new TransactionInputModel
        {
            TransactionDate = transaction.TransactionDate,
            Type = transaction.Type,
            Currency = transaction.Currency,
            Amount = transaction.Amount,
            CategoryId = transaction.CategoryId,
            AccountId = transaction.AccountId,
            Note = transaction.Note
        };
    }

    /// <inheritdoc />
    public async Task<TransactionFormOptionsViewModel> GetFormOptionsAsync(
        TransactionType? type = null,
        string? currency = null,
        CancellationToken cancellationToken = default)
    {
        return await formOptionsService.GetOptionsAsync(type, currency, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TransactionResult> CreateAsync(TransactionInputModel input, CancellationToken cancellationToken = default)
    {
        ValidationState validation = await ValidateAsync(input, cancellationToken);
        if (!validation.Result.Succeeded)
        {
            return validation.Result;
        }

        string? note = sanitizer.SanitizePlainText(input.Note);
        List<DateTimeOffset> duplicateCreatedAts = await dbContext.Transactions
            .Where(transaction =>
            !transaction.IsDeleted
            && transaction.TransactionDate == input.TransactionDate
            && transaction.Type == input.Type
            && transaction.Currency == validation.Currency
            && transaction.AmountMinorUnits == validation.AmountMinorUnits
            && transaction.CategoryId == input.CategoryId
            && transaction.AccountId == input.AccountId
            && transaction.Note == note)
            .Select(transaction => transaction.CreatedAtUtc)
            .ToListAsync(cancellationToken);
        bool duplicateExists = duplicateCreatedAts.Any(createdAt => createdAt >= dateService.UtcNow.AddSeconds(-3));

        if (duplicateExists)
        {
            return TransactionResult.Success();
        }

        await using IDbContextTransaction transactionScope = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        DateTimeOffset nowUtc = dateService.UtcNow;
        var transaction = new Transaction
        {
            TransactionDate = input.TransactionDate,
            Type = input.Type,
            Currency = validation.Currency,
            AmountMinorUnits = validation.AmountMinorUnits,
            CategoryId = input.CategoryId,
            AccountId = input.AccountId,
            Note = note,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc,
            LastChangeSummary = CreateChangeSummary("新增", validation.Currency, input.Amount, note)
        };

        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.RecordAsync(
            AuditEventType.TransactionCreated,
            nameof(Transaction),
            transaction.Id.ToString(),
            transaction.LastChangeSummary,
            cancellationToken: cancellationToken);
        if (input.Type == TransactionType.Expense && budgetService is not null)
        {
            await budgetService.AuditWarningForCategoryMonthAsync(input.CategoryId, input.TransactionDate, cancellationToken);
        }

        await transactionScope.CommitAsync(cancellationToken);

        return TransactionResult.Success(transaction.Id);
    }

    /// <inheritdoc />
    public async Task<TransactionResult> UpdateAsync(long id, TransactionInputModel input, CancellationToken cancellationToken = default)
    {
        Transaction? transaction = await dbContext.Transactions.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (transaction is null)
        {
            TransactionResult missing = TransactionResult.Failure();
            missing.AddError(string.Empty, "找不到交易紀錄。");
            return missing;
        }

        ValidationState validation = await ValidateAsync(input, cancellationToken);
        if (!validation.Result.Succeeded)
        {
            return validation.Result;
        }

        await using IDbContextTransaction transactionScope = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        string? note = sanitizer.SanitizePlainText(input.Note);
        transaction.TransactionDate = input.TransactionDate;
        transaction.Type = input.Type;
        transaction.Currency = validation.Currency;
        transaction.AmountMinorUnits = validation.AmountMinorUnits;
        transaction.CategoryId = input.CategoryId;
        transaction.AccountId = input.AccountId;
        transaction.Note = note;
        transaction.UpdatedAtUtc = dateService.UtcNow;
        transaction.LastChangeSummary = CreateChangeSummary("更新", validation.Currency, input.Amount, note);

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.RecordAsync(
            AuditEventType.TransactionUpdated,
            nameof(Transaction),
            transaction.Id.ToString(),
            transaction.LastChangeSummary,
            cancellationToken: cancellationToken);
        if (input.Type == TransactionType.Expense && budgetService is not null)
        {
            await budgetService.AuditWarningForCategoryMonthAsync(input.CategoryId, input.TransactionDate, cancellationToken);
        }

        await transactionScope.CommitAsync(cancellationToken);

        return TransactionResult.Success(transaction.Id);
    }

    /// <inheritdoc />
    public async Task<TransactionResult> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        Transaction? transaction = await dbContext.Transactions.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (transaction is null)
        {
            TransactionResult missing = TransactionResult.Failure();
            missing.AddError(string.Empty, "找不到交易紀錄。");
            return missing;
        }

        await using IDbContextTransaction transactionScope = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        transaction.IsDeleted = true;
        transaction.DeletedAtUtc = dateService.UtcNow;
        transaction.UpdatedAtUtc = dateService.UtcNow;
        transaction.DeletionSummary = CreateChangeSummary("刪除", transaction.Currency, transaction.Amount, transaction.Note);

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.RecordAsync(
            AuditEventType.TransactionDeleted,
            nameof(Transaction),
            transaction.Id.ToString(),
            transaction.DeletionSummary,
            severity: "Warning",
            cancellationToken: cancellationToken);
        await transactionScope.CommitAsync(cancellationToken);

        return TransactionResult.Success(transaction.Id);
    }

    private async Task<ValidationState> ValidateAsync(TransactionInputModel input, CancellationToken cancellationToken)
    {
        TransactionResult result = TransactionResult.Success();
        long amountMinorUnits = 0;
        string normalizedCurrency = SupportedCurrency.LegacyDefaultCode;
        try
        {
            amountMinorUnits = MoneyMinorUnitConverter.ToMinorUnits(input.Amount);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            result.AddError(nameof(TransactionInputModel.Amount), exception.Message);
        }
        catch (OverflowException exception)
        {
            result.AddError(nameof(TransactionInputModel.Amount), exception.Message);
        }

        if (!SupportedCurrency.TryNormalize(input.Currency, out string? currency))
        {
            result.AddError(
                nameof(TransactionInputModel.Currency),
                string.IsNullOrWhiteSpace(input.Currency)
                    ? FinancialValidationMessages.CurrencyRequired
                    : FinancialValidationMessages.CurrencyUnsupported);
        }
        else
        {
            normalizedCurrency = currency!;
        }

        if (input.TransactionDate > dateService.Today)
        {
            result.AddError(nameof(TransactionInputModel.TransactionDate), FinancialValidationMessages.DateCannotBeFuture);
        }

        Category? category = await dbContext.Categories.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == input.CategoryId && !item.IsArchived, cancellationToken);
        if (category is null || category.Type != input.Type)
        {
            result.AddError(nameof(TransactionInputModel.CategoryId), FinancialValidationMessages.CategoryRequired);
        }

        var account = await dbContext.Accounts.AsNoTracking()
            .Where(account => account.Id == input.AccountId && !account.IsArchived)
            .Select(account => new { account.Currency })
            .FirstOrDefaultAsync(cancellationToken);
        if (account is null)
        {
            result.AddError(nameof(TransactionInputModel.AccountId), FinancialValidationMessages.AccountRequired);
        }
        else if (result.Succeeded && account.Currency != normalizedCurrency)
        {
            result.AddError(nameof(TransactionInputModel.AccountId), FinancialValidationMessages.AccountCurrencyMismatch);
        }

        return new ValidationState(result.Errors.Count == 0 ? TransactionResult.Success() : result, amountMinorUnits, normalizedCurrency);
    }

    private string CreateChangeSummary(string action, string currency, decimal amount, string? note)
    {
        return $"{action}交易，金額 {currency} {maskingPolicy.MaskAmount(amount)}，備註 {maskingPolicy.MaskText(note)}";
    }

    private static TransactionListItemViewModel ToListItem(Transaction transaction)
    {
        return new TransactionListItemViewModel
        {
            Id = transaction.Id,
            TransactionDate = transaction.TransactionDate,
            Type = transaction.Type,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            CategoryName = transaction.Category.Name,
            AccountName = transaction.Account.Name,
            Note = transaction.Note
        };
    }

    private sealed record ValidationState(TransactionResult Result, long AmountMinorUnits, string Currency);
}
