using BookKeeping2.Data;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Services.Common;
using BookKeeping2.ViewModels.Transactions;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping2.Services.Transactions;

/// <summary>
/// EF Core transaction query implementation.
/// </summary>
public sealed class TransactionQueryService : ITransactionQueryService
{
    private readonly AppDbContext dbContext;

    /// <summary>
    /// Initializes a transaction query service.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    public TransactionQueryService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<PagedTransactionListViewModel> SearchAsync(TransactionQuery query, CancellationToken cancellationToken = default)
    {
        int page = Math.Max(query.Page, 1);
        int pageSize = Math.Clamp(query.PageSize, 1, 200);
        IQueryable<Transaction> transactions = dbContext.Transactions
            .AsNoTracking()
            .Include(transaction => transaction.Category)
            .Include(transaction => transaction.Account)
            .Where(transaction => !transaction.IsDeleted);

        if (query.StartDate.HasValue)
        {
            transactions = transactions.Where(transaction => transaction.TransactionDate >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            transactions = transactions.Where(transaction => transaction.TransactionDate <= query.EndDate.Value);
        }

        if (query.CategoryId.HasValue)
        {
            transactions = transactions.Where(transaction => transaction.CategoryId == query.CategoryId.Value);
        }

        if (query.AccountId.HasValue)
        {
            transactions = transactions.Where(transaction => transaction.AccountId == query.AccountId.Value);
        }

        if (SupportedCurrency.TryNormalize(query.Currency, out string? currency))
        {
            transactions = transactions.Where(transaction => transaction.Currency == currency);
        }

        if (query.MinAmount.HasValue)
        {
            long minMinorUnits = MoneyMinorUnitConverter.ToMinorUnits(query.MinAmount.Value, requirePositive: false);
            transactions = transactions.Where(transaction => transaction.AmountMinorUnits >= minMinorUnits);
        }

        if (query.MaxAmount.HasValue)
        {
            long maxMinorUnits = MoneyMinorUnitConverter.ToMinorUnits(query.MaxAmount.Value, requirePositive: false);
            transactions = transactions.Where(transaction => transaction.AmountMinorUnits <= maxMinorUnits);
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            string keyword = query.Keyword.Trim();
            transactions = transactions.Where(transaction =>
                (transaction.Note != null && transaction.Note.Contains(keyword))
                || transaction.Category.Name.Contains(keyword)
                || transaction.Account.Name.Contains(keyword));
        }

        int totalCount = await transactions.CountAsync(cancellationToken);
        List<Transaction> pageItems = await transactions
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedTransactionListViewModel
        {
            Items = pageItems.Select(ToListItem).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
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
}
