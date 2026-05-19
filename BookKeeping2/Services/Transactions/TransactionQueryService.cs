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
            .Where(transaction => !transaction.IsDeleted);
        var transfers = dbContext.AccountTransfers
            .AsNoTracking()
            .Where(transfer => !transfer.IsDeleted);

        if (query.StartDate.HasValue)
        {
            transactions = transactions.Where(transaction => transaction.TransactionDate >= query.StartDate.Value);
            transfers = transfers.Where(transfer => transfer.TransferDate >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            transactions = transactions.Where(transaction => transaction.TransactionDate <= query.EndDate.Value);
            transfers = transfers.Where(transfer => transfer.TransferDate <= query.EndDate.Value);
        }

        if (query.CategoryId.HasValue)
        {
            transactions = transactions.Where(transaction => transaction.CategoryId == query.CategoryId.Value);
            transfers = transfers.Where(_ => false);
        }

        if (query.AccountId.HasValue)
        {
            transactions = transactions.Where(transaction => transaction.AccountId == query.AccountId.Value);
            transfers = transfers.Where(transfer => transfer.FromAccountId == query.AccountId.Value || transfer.ToAccountId == query.AccountId.Value);
        }

        if (SupportedCurrency.TryNormalize(query.Currency, out string? currency))
        {
            transactions = transactions.Where(transaction => transaction.Currency == currency);
            transfers = transfers.Where(transfer => transfer.Currency == currency);
        }

        if (query.MinAmount.HasValue)
        {
            long minMinorUnits = MoneyMinorUnitConverter.ToMinorUnits(query.MinAmount.Value, requirePositive: false);
            transactions = transactions.Where(transaction => transaction.AmountMinorUnits >= minMinorUnits);
            transfers = transfers.Where(transfer => transfer.AmountMinorUnits >= minMinorUnits);
        }

        if (query.MaxAmount.HasValue)
        {
            long maxMinorUnits = MoneyMinorUnitConverter.ToMinorUnits(query.MaxAmount.Value, requirePositive: false);
            transactions = transactions.Where(transaction => transaction.AmountMinorUnits <= maxMinorUnits);
            transfers = transfers.Where(transfer => transfer.AmountMinorUnits <= maxMinorUnits);
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            string keyword = query.Keyword.Trim();
            transactions = transactions.Where(transaction =>
                (transaction.Note != null && transaction.Note.Contains(keyword))
                || transaction.Category.Name.Contains(keyword)
                || transaction.Account.Name.Contains(keyword));
            transfers = transfers.Where(transfer =>
                (transfer.Note != null && transfer.Note.Contains(keyword))
                || transfer.FromAccount.Name.Contains(keyword)
                || transfer.ToAccount.Name.Contains(keyword));
        }

        var transactionItems = await transactions
            .Select(transaction => new
            {
                transaction.Id,
                transaction.TransactionDate,
                transaction.Type,
                transaction.AmountMinorUnits,
                transaction.Currency,
                CategoryName = transaction.Category.Name,
                AccountName = transaction.Account.Name,
                transaction.Note
            })
            .ToListAsync(cancellationToken);

        var transferItems = await transfers
            .Select(transfer => new
            {
                transfer.Id,
                transfer.TransferDate,
                transfer.AmountMinorUnits,
                transfer.Currency,
                FromAccountName = transfer.FromAccount.Name,
                ToAccountName = transfer.ToAccount.Name,
                transfer.Note
            })
            .ToListAsync(cancellationToken);

        List<TransactionTimelineItemViewModel> timelineItems = transactionItems
            .Select(transaction => new TransactionTimelineItemViewModel
            {
                Id = transaction.Id,
                RecordKind = transaction.Type.ToString(),
                TransactionDate = transaction.TransactionDate,
                Type = transaction.Type,
                Amount = MoneyMinorUnitConverter.FromMinorUnits(transaction.AmountMinorUnits),
                Currency = transaction.Currency,
                CategoryName = transaction.CategoryName,
                AccountName = transaction.AccountName,
                Note = transaction.Note,
                EditPage = "/Transactions/Edit",
                DeletePage = "/Transactions/Delete"
            })
            .Concat(transferItems.Select(transfer => new TransactionTimelineItemViewModel
            {
                Id = transfer.Id,
                RecordKind = "Transfer",
                TransactionDate = transfer.TransferDate,
                Amount = MoneyMinorUnitConverter.FromMinorUnits(transfer.AmountMinorUnits),
                Currency = transfer.Currency,
                CategoryName = string.Empty,
                AccountName = $"{transfer.FromAccountName} -> {transfer.ToAccountName}",
                FromAccountName = transfer.FromAccountName,
                ToAccountName = transfer.ToAccountName,
                Note = transfer.Note,
                EditPage = "/Transfers/Edit",
                DeletePage = "/Transfers/Delete"
            }))
            .OrderByDescending(item => item.TransactionDate)
            .ThenByDescending(item => item.Id)
            .ToList();

        int totalCount = timelineItems.Count;
        return new PagedTransactionListViewModel
        {
            Items = timelineItems
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
