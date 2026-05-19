using BookKeeping2.Data;
using BookKeeping2.Models.AccountTransfers;
using BookKeeping2.Models.Audit;
using BookKeeping2.Models.Common;
using BookKeeping2.Services.Audit;
using BookKeeping2.Services.Common;
using BookKeeping2.Services.Time;
using BookKeeping2.Validation;
using BookKeeping2.ViewModels.AccountTransfers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookKeeping2.Services.AccountTransfers;

/// <summary>
/// EF Core implementation of account transfer workflows.
/// </summary>
public sealed class AccountTransferService : IAccountTransferService
{
    private readonly AppDbContext dbContext;
    private readonly ITaipeiDateService dateService;
    private readonly IAuditService auditService;
    private readonly AuditLogMaskingPolicy maskingPolicy;
    private readonly TextInputSanitizer sanitizer;

    /// <summary>
    /// Initializes a new account transfer service.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    /// <param name="dateService">The Taipei date service.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="maskingPolicy">The audit masking policy.</param>
    /// <param name="sanitizer">The text sanitizer.</param>
    public AccountTransferService(
        AppDbContext dbContext,
        ITaipeiDateService dateService,
        IAuditService auditService,
        AuditLogMaskingPolicy maskingPolicy,
        TextInputSanitizer? sanitizer = null)
    {
        this.dbContext = dbContext;
        this.dateService = dateService;
        this.auditService = auditService;
        this.maskingPolicy = maskingPolicy;
        this.sanitizer = sanitizer ?? new TextInputSanitizer();
    }

    /// <inheritdoc />
    public async Task<AccountTransferDetailsViewModel?> GetDetailsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await dbContext.AccountTransfers
            .AsNoTracking()
            .Where(transfer => transfer.Id == id && !transfer.IsDeleted)
            .Select(transfer => new AccountTransferDetailsViewModel
            {
                Id = transfer.Id,
                TransferDate = transfer.TransferDate,
                Currency = transfer.Currency,
                Amount = transfer.Amount,
                FromAccountName = transfer.FromAccount.Name,
                ToAccountName = transfer.ToAccount.Name,
                Note = transfer.Note
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AccountTransferInputModel?> GetForEditAsync(long id, CancellationToken cancellationToken = default)
    {
        return await dbContext.AccountTransfers
            .AsNoTracking()
            .Where(transfer => transfer.Id == id && !transfer.IsDeleted)
            .Select(transfer => new AccountTransferInputModel
            {
                TransferDate = transfer.TransferDate,
                Currency = transfer.Currency,
                Amount = transfer.Amount,
                FromAccountId = transfer.FromAccountId,
                ToAccountId = transfer.ToAccountId,
                Note = transfer.Note
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AccountTransferFormOptionsViewModel> GetFormOptionsAsync(
        string? currency = null,
        CancellationToken cancellationToken = default)
    {
        bool hasCurrency = SupportedCurrency.TryNormalize(currency, out string? normalizedCurrency);
        var accounts = dbContext.Accounts.AsNoTracking().Where(account => !account.IsArchived);
        if (hasCurrency)
        {
            accounts = accounts.Where(account => account.Currency == normalizedCurrency);
        }

        return new AccountTransferFormOptionsViewModel
        {
            Currencies = SupportedCurrency.Options
                .Select(option => new SelectListItem($"{option.Code} - {option.DisplayName}", option.Code))
                .ToList(),
            Accounts = await accounts
                .OrderBy(account => account.DisplayOrder)
                .ThenBy(account => account.Name)
                .Select(account => new AccountTransferAccountOptionViewModel
                {
                    Id = account.Id,
                    Name = account.Name,
                    Currency = account.Currency
                })
                .ToListAsync(cancellationToken)
        };
    }

    /// <inheritdoc />
    public async Task<AccountTransferResult> CreateAsync(AccountTransferInputModel input, CancellationToken cancellationToken = default)
    {
        string? submissionToken = input.SubmissionToken?.Trim();
        if (string.IsNullOrWhiteSpace(submissionToken))
        {
            AccountTransferResult missingToken = AccountTransferResult.Failure();
            missingToken.AddError(nameof(AccountTransferInputModel.SubmissionToken), "表單提交識別碼無效，請重新開啟表單。");
            return missingToken;
        }

        AccountTransfer? existingToken = await dbContext.AccountTransfers
            .AsNoTracking()
            .FirstOrDefaultAsync(transfer => transfer.SubmissionToken == submissionToken, cancellationToken);
        if (existingToken is not null)
        {
            return AccountTransferResult.Success(existingToken.Id);
        }

        ValidationState validation = await ValidateAsync(input, cancellationToken);
        if (!validation.Result.Succeeded)
        {
            return validation.Result;
        }

        await using IDbContextTransaction transactionScope = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        DateTimeOffset nowUtc = dateService.UtcNow;
        string? note = sanitizer.SanitizePlainText(input.Note);
        var transfer = new AccountTransfer
        {
            TransferDate = input.TransferDate,
            Currency = validation.Currency,
            AmountMinorUnits = validation.AmountMinorUnits,
            FromAccountId = input.FromAccountId,
            ToAccountId = input.ToAccountId,
            Note = note,
            SubmissionToken = submissionToken,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc,
            LastChangeSummary = CreateChangeSummary("新增", validation.Currency, input.Amount, validation.FromAccountName, validation.ToAccountName, note)
        };

        dbContext.AccountTransfers.Add(transfer);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.RecordAsync(
            AuditEventType.TransferCreated,
            nameof(AccountTransfer),
            transfer.Id.ToString(),
            transfer.LastChangeSummary,
            cancellationToken: cancellationToken);
        await transactionScope.CommitAsync(cancellationToken);

        return AccountTransferResult.Success(transfer.Id);
    }

    /// <inheritdoc />
    public async Task<AccountTransferResult> UpdateAsync(
        long id,
        AccountTransferInputModel input,
        CancellationToken cancellationToken = default)
    {
        AccountTransfer? transfer = await dbContext.AccountTransfers
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (transfer is null)
        {
            AccountTransferResult missing = AccountTransferResult.Failure();
            missing.AddError(string.Empty, "找不到轉帳紀錄。");
            return missing;
        }

        ValidationState validation = await ValidateAsync(input, cancellationToken);
        if (!validation.Result.Succeeded)
        {
            return validation.Result;
        }

        await using IDbContextTransaction transactionScope = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        string? note = sanitizer.SanitizePlainText(input.Note);
        transfer.TransferDate = input.TransferDate;
        transfer.Currency = validation.Currency;
        transfer.AmountMinorUnits = validation.AmountMinorUnits;
        transfer.FromAccountId = input.FromAccountId;
        transfer.ToAccountId = input.ToAccountId;
        transfer.Note = note;
        transfer.UpdatedAtUtc = dateService.UtcNow;
        transfer.LastChangeSummary = CreateChangeSummary("更新", validation.Currency, input.Amount, validation.FromAccountName, validation.ToAccountName, note);

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.RecordAsync(
            AuditEventType.TransferUpdated,
            nameof(AccountTransfer),
            transfer.Id.ToString(),
            transfer.LastChangeSummary,
            cancellationToken: cancellationToken);
        await transactionScope.CommitAsync(cancellationToken);

        return AccountTransferResult.Success(transfer.Id);
    }

    /// <inheritdoc />
    public async Task<AccountTransferResult> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        AccountTransfer? transfer = await dbContext.AccountTransfers
            .Include(item => item.FromAccount)
            .Include(item => item.ToAccount)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (transfer is null)
        {
            AccountTransferResult missing = AccountTransferResult.Failure();
            missing.AddError(string.Empty, "找不到轉帳紀錄。");
            return missing;
        }

        await using IDbContextTransaction transactionScope = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        transfer.IsDeleted = true;
        transfer.DeletedAtUtc = dateService.UtcNow;
        transfer.UpdatedAtUtc = dateService.UtcNow;
        transfer.DeletionSummary = CreateChangeSummary(
            "刪除",
            transfer.Currency,
            transfer.Amount,
            transfer.FromAccount.Name,
            transfer.ToAccount.Name,
            transfer.Note);

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.RecordAsync(
            AuditEventType.TransferDeleted,
            nameof(AccountTransfer),
            transfer.Id.ToString(),
            transfer.DeletionSummary,
            severity: "Warning",
            cancellationToken: cancellationToken);
        await transactionScope.CommitAsync(cancellationToken);

        return AccountTransferResult.Success(transfer.Id);
    }

    private async Task<ValidationState> ValidateAsync(AccountTransferInputModel input, CancellationToken cancellationToken)
    {
        AccountTransferResult result = AccountTransferResult.Success();
        long amountMinorUnits = 0;
        string normalizedCurrency = SupportedCurrency.LegacyDefaultCode;
        try
        {
            amountMinorUnits = MoneyMinorUnitConverter.ToMinorUnits(input.Amount);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            result.AddError(nameof(AccountTransferInputModel.Amount), exception.Message);
        }
        catch (OverflowException exception)
        {
            result.AddError(nameof(AccountTransferInputModel.Amount), exception.Message);
        }

        if (!SupportedCurrency.TryNormalize(input.Currency, out string? currency))
        {
            result.AddError(
                nameof(AccountTransferInputModel.Currency),
                string.IsNullOrWhiteSpace(input.Currency)
                    ? FinancialValidationMessages.CurrencyRequired
                    : FinancialValidationMessages.CurrencyUnsupported);
        }
        else
        {
            normalizedCurrency = currency!;
        }

        if (input.TransferDate > dateService.Today)
        {
            result.AddError(nameof(AccountTransferInputModel.TransferDate), FinancialValidationMessages.DateCannotBeFuture);
        }

        if (input.FromAccountId == input.ToAccountId && input.FromAccountId > 0)
        {
            result.AddError(nameof(AccountTransferInputModel.ToAccountId), "轉出帳戶與轉入帳戶不可相同。");
        }

        var accountRows = await dbContext.Accounts
            .AsNoTracking()
            .Where(account => !account.IsArchived && (account.Id == input.FromAccountId || account.Id == input.ToAccountId))
            .Select(account => new { account.Id, account.Name, account.Currency })
            .ToListAsync(cancellationToken);
        var fromAccount = accountRows.FirstOrDefault(account => account.Id == input.FromAccountId);
        var toAccount = accountRows.FirstOrDefault(account => account.Id == input.ToAccountId);

        if (fromAccount is null)
        {
            result.AddError(nameof(AccountTransferInputModel.FromAccountId), "請選擇轉出帳戶。");
        }

        if (toAccount is null)
        {
            result.AddError(nameof(AccountTransferInputModel.ToAccountId), "請選擇轉入帳戶。");
        }

        if (fromAccount is not null && toAccount is not null)
        {
            if (fromAccount.Currency != toAccount.Currency || fromAccount.Currency != normalizedCurrency || toAccount.Currency != normalizedCurrency)
            {
                result.AddError(nameof(AccountTransferInputModel.Currency), "轉出帳戶、轉入帳戶與轉帳幣別必須一致。");
            }
        }

        if (!result.Succeeded)
        {
            return new ValidationState(result, amountMinorUnits, normalizedCurrency, fromAccount?.Name ?? string.Empty, toAccount?.Name ?? string.Empty);
        }

        return new ValidationState(result, amountMinorUnits, normalizedCurrency, fromAccount!.Name, toAccount!.Name);
    }

    private string CreateChangeSummary(string action, string currency, decimal amount, string fromAccountName, string toAccountName, string? note)
    {
        return $"{action}轉帳，方向 {maskingPolicy.MaskText(fromAccountName)} -> {maskingPolicy.MaskText(toAccountName)}，金額 {currency} {maskingPolicy.MaskAmount(amount)}，備註 {maskingPolicy.MaskText(note)}";
    }

    private sealed record ValidationState(
        AccountTransferResult Result,
        long AmountMinorUnits,
        string Currency,
        string FromAccountName,
        string ToAccountName);
}
