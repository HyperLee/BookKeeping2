using System.Net;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookKeeping2.Tests.Integration.Pages;

public sealed partial class ThemeModePageTests
{
    private const string HomeRoute = "/";
    private const string PrivacyRoute = "/Privacy";
    private const string ErrorRoute = "/Error";
    private const string AccountsRoute = "/Accounts";
    private const string CategoriesRoute = "/Categories";
    private const string BudgetsRoute = "/Budgets";
    private const string TransactionsRoute = "/Transactions";
    private const string CreateTransactionRoute = "/Transactions/Create";
    private const string CsvImportRoute = "/Csv/Import";
    private const string CsvExportRoute = "/Csv/Export";
    private const string ReportsRoute = "/Reports";
    private const string ThemeControlSelector = "data-theme-mode-control";

    private static readonly string[] StaticNonHomeRoutes =
    [
        PrivacyRoute,
        ErrorRoute,
        AccountsRoute,
        CategoriesRoute,
        BudgetsRoute,
        TransactionsRoute,
        CreateTransactionRoute,
        CsvImportRoute,
        CsvExportRoute,
        ReportsRoute
    ];

    private static async Task<string> GetSuccessfulHtmlAsync(HttpClient client, string route)
    {
        HttpResponseMessage response = await client.GetAsync(route);
        string body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);
        return WebUtility.HtmlDecode(body);
    }

    private static async Task<IReadOnlyList<string>> GetAllUserFacingRoutesAsync(BookKeepingWebApplicationFactory factory)
    {
        long transactionId = await SeedTransactionAsync(factory);
        List<string> routes = [HomeRoute, .. StaticNonHomeRoutes];
        routes.Add($"/Transactions/Edit/{transactionId}");
        routes.Add($"/Transactions/Delete/{transactionId}");
        return routes;
    }

    private static async Task<IReadOnlyList<string>> GetNonHomeUserFacingRoutesAsync(BookKeepingWebApplicationFactory factory)
    {
        long transactionId = await SeedTransactionAsync(factory);
        List<string> routes = [.. StaticNonHomeRoutes];
        routes.Add($"/Transactions/Edit/{transactionId}");
        routes.Add($"/Transactions/Delete/{transactionId}");
        return routes;
    }

    private static void AssertThemeControlRendered(string html)
    {
        Assert.Contains(ThemeControlSelector, html, StringComparison.Ordinal);
    }

    private static void AssertThemeControlNotRendered(string html)
    {
        Assert.DoesNotContain(ThemeControlSelector, html, StringComparison.Ordinal);
    }

    private static async Task<long> SeedTransactionAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Transaction? existing = await context.Transactions.FirstOrDefaultAsync();
        if (existing is not null)
        {
            return existing.Id;
        }

        Category category = await context.Categories.FirstAsync(category => category.Type == TransactionType.Expense && category.Name == "餐飲");
        var account = new Account
        {
            Name = "主題測試現金",
            NormalizedName = "主題測試現金",
            Type = AccountType.Cash,
            IconKey = "wallet",
            Currency = "TWD",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var transaction = new Transaction
        {
            AccountId = account.Id,
            CategoryId = category.Id,
            Type = TransactionType.Expense,
            TransactionDate = TestDataBuilder.DefaultToday,
            Amount = 120m,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "主題模式測試資料"
        };
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();
        return transaction.Id;
    }
}
