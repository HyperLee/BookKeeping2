using BookKeeping2.Data;
using BookKeeping2.Models.Audit;
using BookKeeping2.Models.Budgets;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Services.Audit;
using BookKeeping2.Services.Common;
using BookKeeping2.Services.Time;
using BookKeeping2.ViewModels.Budgets;
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
        var spentTotals = await dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => !transaction.IsDeleted
                && transaction.Type == TransactionType.Expense
                && categoryIds.Contains(transaction.CategoryId)
                && transaction.TransactionDate >= monthStart
                && transaction.TransactionDate <= monthEnd)
            .GroupBy(transaction => transaction.CategoryId)
            .Select(group => new
            {
                CategoryId = group.Key,
                AmountMinorUnits = group.Sum(transaction => transaction.AmountMinorUnits)
            })
            .ToListAsync(cancellationToken);

        return budgets.Select(budget =>
        {
            long spentMinorUnits = spentTotals.FirstOrDefault(total => total.CategoryId == budget.CategoryId)?.AmountMinorUnits ?? 0;
            return ToStatus(budget, spentMinorUnits);
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<BudgetFormOptionsViewModel> GetFormOptionsAsync(CancellationToken cancellationToken = default)
    {
        List<BudgetCategoryOptionViewModel> categories = await dbContext.Categories
            .AsNoTracking()
            .Where(category => category.Type == TransactionType.Expense && !category.IsArchived)
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .Select(category => new BudgetCategoryOptionViewModel
            {
                Id = category.Id,
                Name = category.Name
            })
            .ToListAsync(cancellationToken);

        return new BudgetFormOptionsViewModel { Categories = categories };
    }

    /// <inheritdoc />
    public async Task<BudgetResult> SaveAsync(BudgetInputModel input, CancellationToken cancellationToken = default)
    {
        var result = new BudgetResult();
        DateOnly budgetMonth = NormalizeMonth(input.BudgetMonth);
        long amountMinorUnits = 0;
        try
        {
            amountMinorUnits = MoneyMinorUnitConverter.ToMinorUnits(input.Amount);
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException or OverflowException)
        {
            result.AddError(nameof(BudgetInputModel.Amount), exception.Message);
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

        Budget? budget = await dbContext.Budgets
            .FirstOrDefaultAsync(item => item.CategoryId == input.CategoryId && item.BudgetMonth == budgetMonth, cancellationToken);
        DateTimeOffset nowUtc = dateService.UtcNow;
        if (budget is null)
        {
            budget = new Budget
            {
                CategoryId = input.CategoryId,
                BudgetMonth = budgetMonth,
                CreatedAtUtc = nowUtc
            };
            dbContext.Budgets.Add(budget);
        }

        budget.AmountMinorUnits = amountMinorUnits;
        budget.UpdatedAtUtc = nowUtc;
        await dbContext.SaveChangesAsync(cancellationToken);
        await AuditWarningForCategoryMonthAsync(input.CategoryId, budgetMonth, cancellationToken);

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
    public async Task AuditWarningForCategoryMonthAsync(long categoryId, DateOnly transactionDate, CancellationToken cancellationToken = default)
    {
        if (auditService is null)
        {
            return;
        }

        DateOnly monthStart = NormalizeMonth(transactionDate);
        Budget? budget = await dbContext.Budgets
            .AsNoTracking()
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.CategoryId == categoryId && item.BudgetMonth == monthStart, cancellationToken);
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
            $"預算提醒，分類 {status.CategoryName}，狀態 {status.AlertText}，已使用 {maskingPolicy.MaskAmount(status.SpentAmount)}，預算 {maskingPolicy.MaskAmount(status.Amount)}",
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
