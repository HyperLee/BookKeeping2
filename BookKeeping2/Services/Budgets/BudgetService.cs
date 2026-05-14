using BookKeeping2.Data;
using BookKeeping2.Localization;
using BookKeeping2.Models.Audit;
using BookKeeping2.Models.Budgets;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Services.Audit;
using BookKeeping2.Services.Common;
using BookKeeping2.Services.Time;
using BookKeeping2.ViewModels.Budgets;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping2.Services.Budgets;

/// <summary>
/// EF Core implementation of monthly budget workflows.
/// </summary>
public sealed class BudgetService : IBudgetService
{
    private readonly AppDbContext dbContext;
    private readonly ITaipeiDateService dateService;
    private readonly IAuditService? auditService;
    private readonly AuditLogMaskingPolicy maskingPolicy;

    /// <summary>
    /// Initializes a new budget service.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    /// <param name="dateService">The Taipei date service.</param>
    /// <param name="auditService">The optional audit service.</param>
    /// <param name="maskingPolicy">The audit masking policy.</param>
    public BudgetService(
        AppDbContext dbContext,
        ITaipeiDateService dateService,
        IAuditService? auditService = null,
        AuditLogMaskingPolicy? maskingPolicy = null)
    {
        this.dbContext = dbContext;
        this.dateService = dateService;
        this.auditService = auditService;
        this.maskingPolicy = maskingPolicy ?? new AuditLogMaskingPolicy();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BudgetStatusViewModel>> ListMonthlyAsync(DateOnly budgetMonth, CancellationToken cancellationToken = default)
    {
        DateOnly monthStart = NormalizeMonth(budgetMonth);
        DateOnly monthEnd = monthStart.AddMonths(1).AddDays(-1);

        List<Budget> budgets = await dbContext.Budgets
            .AsNoTracking()
            .Include(budget => budget.Category)
            .Where(budget => budget.BudgetMonth == monthStart && budget.Category.Type == TransactionType.Expense)
            .OrderBy(budget => budget.Category.DisplayOrder)
            .ThenBy(budget => budget.Category.Name)
            .ToListAsync(cancellationToken);

        if (budgets.Count == 0)
        {
            return [];
        }

        long[] categoryIds = budgets.Select(budget => budget.CategoryId).Distinct().ToArray();
        string[] currencies = budgets.Select(budget => budget.Currency).Distinct().ToArray();
        var spentTotals = await dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => !transaction.IsDeleted
                && transaction.Type == TransactionType.Expense
                && categoryIds.Contains(transaction.CategoryId)
                && currencies.Contains(transaction.Currency)
                && transaction.TransactionDate >= monthStart
                && transaction.TransactionDate <= monthEnd)
            .GroupBy(transaction => new { transaction.CategoryId, transaction.Currency })
            .Select(group => new
            {
                group.Key.CategoryId,
                group.Key.Currency,
                AmountMinorUnits = group.Sum(transaction => transaction.AmountMinorUnits)
            })
            .ToListAsync(cancellationToken);

        return budgets.Select(budget =>
        {
            long spentMinorUnits = spentTotals.FirstOrDefault(total => total.CategoryId == budget.CategoryId && total.Currency == budget.Currency)?.AmountMinorUnits ?? 0;
            return ToStatus(budget, spentMinorUnits);
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<BudgetFormOptionsViewModel> GetFormOptionsAsync(CancellationToken cancellationToken = default)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .Where(category => category.Type == TransactionType.Expense && !category.IsArchived)
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .Select(category => new
            {
                category.Id,
                category.Name,
                category.IsDefault
            })
            .ToListAsync(cancellationToken);

        return new BudgetFormOptionsViewModel
        {
            Categories = categories
                .Select(category => new BudgetCategoryOptionViewModel
                {
                    Id = category.Id,
                    Name = SystemDisplayLocalizer.GetCategoryName(category.Name, category.IsDefault)
                })
                .ToList(),
            Currencies = SupportedCurrency.Options
                .Select(option => new SelectListItem($"{option.Code} - {option.DisplayName}", option.Code))
                .ToList()
        };
    }

    /// <inheritdoc />
    public async Task<BudgetResult> SaveAsync(BudgetInputModel input, CancellationToken cancellationToken = default)
    {
        var result = new BudgetResult();
        DateOnly budgetMonth = NormalizeMonth(input.BudgetMonth);
        string normalizedCurrency = SupportedCurrency.LegacyDefaultCode;
        long amountMinorUnits = 0;
        try
        {
            amountMinorUnits = MoneyMinorUnitConverter.ToMinorUnits(input.Amount);
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException or OverflowException)
        {
            result.AddError(nameof(BudgetInputModel.Amount), exception.Message);
        }

        if (!SupportedCurrency.TryNormalize(input.Currency, out string? currency))
        {
            result.AddError(
                nameof(BudgetInputModel.Currency),
                string.IsNullOrWhiteSpace(input.Currency)
                    ? BudgetResult.CurrencyRequiredMessage
                    : BudgetResult.UnsupportedCurrencyMessage);
        }
        else
        {
            normalizedCurrency = currency!;
        }

        Category? category = await dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == input.CategoryId && !item.IsArchived, cancellationToken);
        if (category is null || category.Type != TransactionType.Expense)
        {
            result.AddError(nameof(BudgetInputModel.CategoryId), "請選擇支出分類。");
        }

        if (!result.Succeeded)
        {
            return result;
        }

        bool duplicate = await dbContext.Budgets
            .AnyAsync(item => item.CategoryId == input.CategoryId && item.BudgetMonth == budgetMonth && item.Currency == normalizedCurrency, cancellationToken);
        if (duplicate)
        {
            result.AddError(nameof(BudgetInputModel.Currency), BudgetResult.DuplicateCategoryMonthCurrencyMessage);
            return result;
        }

        DateTimeOffset nowUtc = dateService.UtcNow;
        var budget = new Budget
        {
            CategoryId = input.CategoryId,
            BudgetMonth = budgetMonth,
            Currency = normalizedCurrency,
            AmountMinorUnits = amountMinorUnits,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        };
        dbContext.Budgets.Add(budget);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AuditWarningForCategoryMonthAsync(input.CategoryId, budgetMonth, normalizedCurrency, cancellationToken);

        return BudgetResult.Success(budget.Id);
    }

    /// <inheritdoc />
    public async Task<BudgetResult> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        Budget? budget = await dbContext.Budgets.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (budget is null)
        {
            var missing = new BudgetResult();
            missing.AddError(string.Empty, "找不到預算設定。");
            return missing;
        }

        dbContext.Budgets.Remove(budget);
        await dbContext.SaveChangesAsync(cancellationToken);
        return BudgetResult.Success(id);
    }

    /// <inheritdoc />
    public async Task AuditWarningForCategoryMonthAsync(long categoryId, DateOnly transactionDate, string? currency = null, CancellationToken cancellationToken = default)
    {
        if (auditService is null)
        {
            return;
        }

        DateOnly monthStart = NormalizeMonth(transactionDate);
        if (!SupportedCurrency.TryNormalize(currency, out string? normalizedCurrency))
        {
            normalizedCurrency = SupportedCurrency.LegacyDefaultCode;
        }

        Budget? budget = await dbContext.Budgets
            .AsNoTracking()
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.CategoryId == categoryId && item.BudgetMonth == monthStart && item.Currency == normalizedCurrency, cancellationToken);
        if (budget is null)
        {
            return;
        }

        DateOnly monthEnd = monthStart.AddMonths(1).AddDays(-1);
        long spentMinorUnits = await dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => !transaction.IsDeleted
                && transaction.Type == TransactionType.Expense
                && transaction.CategoryId == categoryId
                && transaction.Currency == normalizedCurrency
                && transaction.TransactionDate >= monthStart
                && transaction.TransactionDate <= monthEnd)
            .SumAsync(transaction => transaction.AmountMinorUnits, cancellationToken);

        BudgetStatusViewModel status = ToStatus(budget, spentMinorUnits);
        if (status.AlertState == BudgetAlertState.Normal)
        {
            return;
        }

        await auditService.RecordAsync(
            AuditEventType.BudgetWarningTriggered,
            nameof(Budget),
            budget.Id.ToString(),
            $"預算提醒，分類 {status.CategoryName}，幣別 {status.Currency}，狀態 {status.AlertText}，已使用 {status.Currency} {maskingPolicy.MaskAmount(status.SpentAmount)}，預算 {status.Currency} {maskingPolicy.MaskAmount(status.Amount)}",
            severity: "Warning",
            cancellationToken: cancellationToken);
    }

    private static BudgetStatusViewModel ToStatus(Budget budget, long spentMinorUnits)
    {
        decimal amount = MoneyMinorUnitConverter.FromMinorUnits(budget.AmountMinorUnits);
        decimal spent = MoneyMinorUnitConverter.FromMinorUnits(spentMinorUnits);
        decimal usageRate = amount == 0m ? 0m : Math.Round(spent / amount, 4, MidpointRounding.AwayFromZero);
        BudgetAlertState alertState = usageRate switch
        {
            > 1m => BudgetAlertState.Exceeded,
            >= 0.8m => BudgetAlertState.NearLimit,
            _ => BudgetAlertState.Normal
        };

        return new BudgetStatusViewModel
        {
            BudgetId = budget.Id,
            CategoryId = budget.CategoryId,
            CategoryName = budget.Category.Name,
            BudgetMonth = budget.BudgetMonth,
            Currency = budget.Currency,
            Amount = amount,
            SpentAmount = spent,
            UsageRate = usageRate,
            RemainingAmount = Math.Max(amount - spent, 0m),
            OverspentAmount = Math.Max(spent - amount, 0m),
            AlertState = alertState
        };
    }

    private static DateOnly NormalizeMonth(DateOnly value)
    {
        return new DateOnly(value.Year, value.Month, 1);
    }
}
