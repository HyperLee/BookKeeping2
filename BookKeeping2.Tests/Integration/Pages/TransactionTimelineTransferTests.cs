using System.Net;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.AccountTransfers;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookKeeping2.Tests.Integration.Pages;

public sealed class TransactionTimelineTransferTests
{
    [Fact]
    public async Task Transactions_index_displays_transfer_rows_with_direction_and_transfer_actions()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();
        (long transferId, _) = await SeedTimelineAsync(factory);

        string page = WebUtility.HtmlDecode(await client.GetStringAsync("/Transactions"));

        Assert.Contains("新增轉帳", page);
        Assert.Contains("轉帳", page);
        Assert.Contains("銀行 -> 現金", page);
        Assert.Contains($"/Transfers/Edit/{transferId}", page);
        Assert.Contains($"/Transfers/Delete/{transferId}", page);
    }

    [Fact]
    public async Task Transactions_index_category_filter_excludes_transfer_rows()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();
        (_, long categoryId) = await SeedTimelineAsync(factory);

        string page = WebUtility.HtmlDecode(await client.GetStringAsync($"/Transactions?Filter.CategoryId={categoryId}"));

        Assert.Contains("午餐", page);
        Assert.DoesNotContain("銀行 -> 現金", page);
    }

    private static async Task<(long TransferId, long CategoryId)> SeedTimelineAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Category food = await context.Categories.FirstAsync(category => category.Type == TransactionType.Expense && category.Name == "餐飲");
        var bank = TestDataBuilder.CreateAccount("銀行", TestDataBuilder.TwdCurrency);
        var cash = TestDataBuilder.CreateAccount("現金", TestDataBuilder.TwdCurrency);
        context.Accounts.AddRange(bank, cash);
        await context.SaveChangesAsync();
        context.Transactions.Add(new Transaction
        {
            AccountId = cash.Id,
            CategoryId = food.Id,
            Type = TransactionType.Expense,
            TransactionDate = TestDataBuilder.DefaultToday,
            Currency = TestDataBuilder.TwdCurrency,
            Amount = 100m,
            Note = "午餐",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "測試"
        });
        var transfer = new AccountTransfer
        {
            FromAccountId = bank.Id,
            ToAccountId = cash.Id,
            TransferDate = TestDataBuilder.DefaultToday,
            Currency = TestDataBuilder.TwdCurrency,
            Amount = 500m,
            Note = "提款",
            SubmissionToken = Guid.NewGuid().ToString("N"),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "測試"
        };
        context.AccountTransfers.Add(transfer);
        await context.SaveChangesAsync();
        return (transfer.Id, food.Id);
    }
}
