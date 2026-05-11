using System.Net;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Budgets;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookKeeping2.Tests.Integration.Pages;

public sealed class HomeBudgetSummaryTests
{
    [Fact]
    public async Task Home_page_shows_current_month_budget_progress_and_alert_within_one_second()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();
        await SeedBudgetAndSpendingAsync(factory);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        string page = WebUtility.HtmlDecode(await client.GetStringAsync("/", timeout.Token));

        Assert.Contains("預算進度", page);
        Assert.Contains("餐飲", page);
        Assert.Contains("80.00%", page);
        Assert.Contains("接近預算上限", page);
    }

    private static async Task SeedBudgetAndSpendingAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Category food = await context.Categories.FirstAsync(category => category.Type == TransactionType.Expense && category.Name == "餐飲");
        var account = new Account
        {
            Name = "現金",
            NormalizedName = "現金",
            Type = AccountType.Cash,
            IconKey = "wallet",
            Currency = "TWD",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        context.Budgets.Add(new Budget
        {
            CategoryId = food.Id,
            BudgetMonth = new DateOnly(2026, 5, 1),
            Amount = 5_000m,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });
        context.Transactions.Add(new Transaction
        {
            AccountId = account.Id,
            CategoryId = food.Id,
            Type = TransactionType.Expense,
            TransactionDate = new DateOnly(2026, 5, 5),
            Amount = 4_000m,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "測試"
        });
        await context.SaveChangesAsync();
    }
}
