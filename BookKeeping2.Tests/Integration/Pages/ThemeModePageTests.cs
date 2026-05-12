using System.Net;
using System.Text.RegularExpressions;
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

    [Fact]
    public async Task Home_page_renders_theme_mode_radio_group()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();

        string html = await GetSuccessfulHtmlAsync(client, HomeRoute);

        Assert.Contains("<fieldset", html, StringComparison.Ordinal);
        AssertThemeControlRendered(html);
        AssertRadioValueRendered(html, "light");
        AssertRadioValueRendered(html, "dark");
        AssertRadioValueRendered(html, "system");
        Assert.Contains("亮色模式", html, StringComparison.Ordinal);
        Assert.Contains("暗黑模式", html, StringComparison.Ordinal);
        Assert.Contains("跟隨系統", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Non_home_pages_apply_shared_theme_without_rendering_theme_control()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();

        foreach (string route in await GetNonHomeUserFacingRoutesAsync(factory))
        {
            string html = await GetSuccessfulHtmlAsync(client, route);

            Assert.Contains("data-bs-theme", html, StringComparison.Ordinal);
            AssertThemeControlNotRendered(html);
        }
    }

    [Fact]
    public async Task Layout_places_pre_paint_theme_script_before_bootstrap_css_and_preserves_site_script()
    {
        string layout = ReadProjectFile("BookKeeping2", "Pages", "Shared", "_Layout.cshtml");
        int prePaintScriptIndex = layout.IndexOf("bookkeeping.theme.mode", StringComparison.Ordinal);
        int bootstrapCssIndex = layout.IndexOf("~/lib/bootstrap/dist/css/bootstrap.min.css", StringComparison.Ordinal);
        int siteScriptIndex = layout.IndexOf("~/js/site.js", StringComparison.Ordinal);

        Assert.True(prePaintScriptIndex >= 0, "The layout should render a pre-paint theme initialization script.");
        Assert.True(bootstrapCssIndex >= 0, "The layout should render Bootstrap CSS.");
        Assert.True(prePaintScriptIndex < bootstrapCssIndex, "The pre-paint theme script must appear before Bootstrap CSS.");
        Assert.True(siteScriptIndex >= 0, "The shared site.js reference should remain rendered.");
    }

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

    private static void AssertRadioValueRendered(string html, string value)
    {
        Assert.Matches(ThemeModeRadioRegex(value), html);
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

    private static Regex ThemeModeRadioRegex(string value)
    {
        return new Regex(
            $"""<input(?=[^>]*\btype="radio")(?=[^>]*\bname="themeMode")(?=[^>]*\bvalue="{Regex.Escape(value)}")[^>]*>""",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static string ReadProjectFile(params string[] pathParts)
    {
        string path = Path.Combine([FindRepositoryRoot(), .. pathParts]);
        return File.ReadAllText(path);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "BookKeeping2.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate repository root from test output directory.");
    }
}
