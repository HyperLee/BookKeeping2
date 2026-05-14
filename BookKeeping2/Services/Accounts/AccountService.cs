using BookKeeping2.Data;
using BookKeeping2.Data.SeedData;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Common;
using BookKeeping2.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping2.Services.Accounts;

/// <summary>
/// EF Core implementation of account management.
/// </summary>
public sealed class AccountService : IAccountService
{
    private readonly AppDbContext dbContext;

    /// <summary>
    /// Initializes an account service.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    public AccountService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<AccountResult> CreateAsync(
        string name,
        AccountType type,
        decimal openingBalance,
        string? currency,
        string iconKey = "wallet",
        int displayOrder = 0,
        CancellationToken cancellationToken = default)
    {
        var result = new AccountResult();
        string trimmed = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            result.AddError(nameof(Account.Name), "請輸入帳戶名稱。");
            return result;
        }

        string normalizedName = DefaultSeedData.NormalizeName(trimmed);
        bool duplicate = await dbContext.Accounts.AnyAsync(account => account.NormalizedName == normalizedName, cancellationToken);
        if (duplicate)
        {
            result.AddError(nameof(Account.Name), "已有相同名稱的帳戶。");
            return result;
        }

        if (!SupportedCurrency.TryNormalize(currency, out string? normalizedCurrency))
        {
            result.AddError(
                nameof(Account.Currency),
                string.IsNullOrWhiteSpace(currency) ? "請選擇幣別。" : "幣別不支援，請選擇 TWD、USD、JPY、EUR 或 GBP。");
            return result;
        }

        long openingBalanceMinorUnits;
        try
        {
            openingBalanceMinorUnits = MoneyMinorUnitConverter.ToMinorUnits(openingBalance, requirePositive: false);
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException or OverflowException)
        {
            result.AddError(nameof(Account.OpeningBalance), exception.Message);
            return result;
        }

        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        dbContext.Accounts.Add(new Account
        {
            Name = trimmed,
            NormalizedName = normalizedName,
            Type = type,
            OpeningBalanceMinorUnits = openingBalanceMinorUnits,
            IconKey = string.IsNullOrWhiteSpace(iconKey) ? "wallet" : iconKey.Trim(),
            DisplayOrder = displayOrder,
            Currency = normalizedCurrency!,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public async Task<AccountResult> UpdateAsync(
        long id,
        string name,
        AccountType type,
        decimal openingBalance,
        string? currency,
        string iconKey = "wallet",
        int displayOrder = 0,
        CancellationToken cancellationToken = default)
    {
        var result = new AccountResult();
        Account? account = await dbContext.Accounts.FirstOrDefaultAsync(account => account.Id == id, cancellationToken);
        if (account is null)
        {
            result.AddError(string.Empty, "找不到帳戶。");
            return result;
        }

        if (!SupportedCurrency.TryNormalize(currency, out string? normalizedCurrency))
        {
            result.AddError(
                nameof(Account.Currency),
                string.IsNullOrWhiteSpace(currency) ? "請選擇幣別。" : "幣別不支援，請選擇 TWD、USD、JPY、EUR 或 GBP。");
            return result;
        }

        if (!string.Equals(account.Currency, normalizedCurrency, StringComparison.Ordinal))
        {
            result.AddError(nameof(Account.Currency), "帳戶建立後不可修改幣別。");
            return result;
        }

        string trimmed = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            result.AddError(nameof(Account.Name), "請輸入帳戶名稱。");
            return result;
        }

        string normalizedName = DefaultSeedData.NormalizeName(trimmed);
        bool duplicate = await dbContext.Accounts.AnyAsync(
            candidate => candidate.Id != id && candidate.NormalizedName == normalizedName,
            cancellationToken);
        if (duplicate)
        {
            result.AddError(nameof(Account.Name), "已有相同名稱的帳戶。");
            return result;
        }

        long openingBalanceMinorUnits;
        try
        {
            openingBalanceMinorUnits = MoneyMinorUnitConverter.ToMinorUnits(openingBalance, requirePositive: false);
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException or OverflowException)
        {
            result.AddError(nameof(Account.OpeningBalance), exception.Message);
            return result;
        }

        account.Name = trimmed;
        account.NormalizedName = normalizedName;
        account.Type = type;
        account.OpeningBalanceMinorUnits = openingBalanceMinorUnits;
        account.IconKey = string.IsNullOrWhiteSpace(iconKey) ? "wallet" : iconKey.Trim();
        account.DisplayOrder = displayOrder;
        account.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AccountBalanceSummary>> GetBalanceSummariesAsync(bool includeArchived = true, CancellationToken cancellationToken = default)
    {
        var accounts = await dbContext.Accounts
            .AsNoTracking()
            .Where(account => includeArchived || !account.IsArchived)
            .OrderBy(account => account.DisplayOrder)
            .ThenBy(account => account.Name)
            .ToListAsync(cancellationToken);

        var transactionTotals = await dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => !transaction.IsDeleted)
            .GroupBy(transaction => new { transaction.AccountId, transaction.Currency })
            .Select(group => new
            {
                group.Key.AccountId,
                group.Key.Currency,
                Income = group.Where(transaction => transaction.Type == TransactionType.Income).Sum(transaction => transaction.AmountMinorUnits),
                Expense = group.Where(transaction => transaction.Type == TransactionType.Expense).Sum(transaction => transaction.AmountMinorUnits)
            })
            .ToListAsync(cancellationToken);

        return accounts.Select(account =>
        {
            var totals = transactionTotals.FirstOrDefault(total => total.AccountId == account.Id && total.Currency == account.Currency);
            long currentMinorUnits = account.OpeningBalanceMinorUnits + (totals?.Income ?? 0) - (totals?.Expense ?? 0);
            return new AccountBalanceSummary
            {
                AccountId = account.Id,
                Name = account.Name,
                Type = account.Type,
                Currency = account.Currency,
                IsArchived = account.IsArchived,
                CurrentBalance = MoneyMinorUnitConverter.FromMinorUnits(currentMinorUnits)
            };
        }).ToList();
    }
}
