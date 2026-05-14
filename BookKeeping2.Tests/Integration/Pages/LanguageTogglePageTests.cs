using System.Net;
using System.Text.RegularExpressions;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookKeeping2.Tests.Integration.Pages;

public sealed partial class LanguageTogglePageTests
{
    private const string HomeRoute = "/";
    private const string LanguageControlSelector = "data-language-control";

    private static readonly Dictionary<string, string> EnglishRouteExpectations = new(StringComparer.Ordinal)
    {
        [HomeRoute] = "Monthly Income",
        ["/Transactions"] = "Transaction Details",
        ["/Categories"] = "Category Management",
        ["/Accounts"] = "Account Management",
        ["/Budgets"] = "Budget Management",
        ["/Reports"] = "Monthly Report",
        ["/Csv/Import"] = "CSV Import",
        ["/Csv/Export"] = "CSV Export",
        ["/Privacy"] = "Privacy Notice",
        ["/Error"] = "An error occurred"
    };

    [Fact]
    public async Task Home_page_renders_language_control_with_default_selected_option()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();

        string html = await GetSuccessfulHtmlAsync(client, HomeRoute);

        Assert.Contains(LanguageControlSelector, html, StringComparison.Ordinal);
        Assert.Contains("<legend", html, StringComparison.Ordinal);
        Assert.Contains("介面語言", html, StringComparison.Ordinal);
        Assert.Matches(LanguageRadioRegex("zh-TW", requiredChecked: true), html);
        Assert.Matches(LanguageRadioRegex("en", requiredChecked: false), html);
        Assert.Contains("繁體中文", html, StringComparison.Ordinal);
        Assert.Contains("English", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Non_home_pages_apply_language_without_rendering_language_control()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();

        foreach (string route in EnglishRouteExpectations.Keys.Where(route => route != HomeRoute))
        {
            string html = await GetSuccessfulHtmlAsync(client, route);

            Assert.DoesNotContain(LanguageControlSelector, html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task English_cookie_renders_home_and_directly_browsable_pages_in_english()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", "bookkeeping.ui.language=en");

        foreach ((string route, string expectedText) in EnglishRouteExpectations)
        {
            string html = await GetSuccessfulHtmlAsync(client, route);

            Assert.Contains("lang=\"en\"", html, StringComparison.Ordinal);
            Assert.Contains(expectedText, html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Language_post_redirects_and_writes_selected_language_cookie()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient(new() { AllowAutoRedirect = false });
        string html = await GetSuccessfulHtmlAsync(client, HomeRoute);
        string token = ExtractRequestVerificationToken(html);

        HttpResponseMessage response = await client.PostAsync("/?handler=Language", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["uiLanguage"] = "en"
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), value => value.StartsWith("bookkeeping.ui.language=en", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Missing_or_invalid_language_cookie_renders_default_traditional_chinese()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient missingCookieClient = factory.CreateClient();
        HttpClient invalidCookieClient = factory.CreateClient();
        invalidCookieClient.DefaultRequestHeaders.Add("Cookie", "bookkeeping.ui.language=fr");

        string missingCookieHtml = await GetSuccessfulHtmlAsync(missingCookieClient, HomeRoute);
        string invalidCookieHtml = await GetSuccessfulHtmlAsync(invalidCookieClient, HomeRoute);

        Assert.Contains("lang=\"zh-Hant-TW\"", missingCookieHtml, StringComparison.Ordinal);
        Assert.Contains("本月收入", missingCookieHtml, StringComparison.Ordinal);
        Assert.Matches(LanguageRadioRegex("zh-TW", requiredChecked: true), missingCookieHtml);
        Assert.Contains("lang=\"zh-Hant-TW\"", invalidCookieHtml, StringComparison.Ordinal);
        Assert.Contains("本月收入", invalidCookieHtml, StringComparison.Ordinal);
        Assert.Matches(LanguageRadioRegex("zh-TW", requiredChecked: true), invalidCookieHtml);
    }

    [Fact]
    public async Task Accept_language_header_does_not_switch_default_ui_to_english()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();
        using HttpRequestMessage request = new(HttpMethod.Get, HomeRoute);
        request.Headers.AcceptLanguage.ParseAdd("en-US,en;q=0.9");

        HttpResponseMessage response = await client.SendAsync(request);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.True(response.IsSuccessStatusCode, html);
        Assert.Contains("lang=\"zh-Hant-TW\"", html, StringComparison.Ordinal);
        Assert.Contains("本月收入", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Monthly Income", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Language_post_writes_one_year_secure_http_only_cookie_attributes()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        string html = await GetSuccessfulHtmlAsync(client, HomeRoute);
        string token = ExtractRequestVerificationToken(html);

        HttpResponseMessage response = await client.PostAsync("/?handler=Language", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["uiLanguage"] = "en"
        }));

        string cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"), value => value.StartsWith("bookkeeping.ui.language=en", StringComparison.Ordinal));
        Assert.Contains("path=/", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("expires=", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Invalid_language_post_value_writes_default_language_cookie()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient(new() { AllowAutoRedirect = false });
        string html = await GetSuccessfulHtmlAsync(client, HomeRoute);
        string token = ExtractRequestVerificationToken(html);

        HttpResponseMessage response = await client.PostAsync("/?handler=Language", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["uiLanguage"] = "fr"
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), value => value.StartsWith("bookkeeping.ui.language=zh-TW", StringComparison.Ordinal));
    }

    [Fact]
    public async Task English_transaction_form_localizes_display_names_but_keeps_validation_messages_traditional_chinese()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", "bookkeeping.ui.language=en");
        string createPage = await GetSuccessfulHtmlAsync(client, "/Transactions/Create");
        string token = ExtractRequestVerificationToken(createPage);

        HttpResponseMessage response = await client.PostAsync("/Transactions/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Input.TransactionDate"] = "2026-05-11",
            ["Input.Type"] = "Expense",
            ["Input.Amount"] = "0",
            ["Input.CategoryId"] = "0",
            ["Input.AccountId"] = "0"
        }));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.True(response.IsSuccessStatusCode, html);
        Assert.Contains("Date", html, StringComparison.Ordinal);
        Assert.Contains("Amount", html, StringComparison.Ordinal);
        Assert.Contains("Category", html, StringComparison.Ordinal);
        Assert.Contains("金額不可超過 999,999,999.99。", html, StringComparison.Ordinal);
        Assert.Contains("請選擇有效分類。", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task English_account_post_localizes_success_status_and_preserves_custom_user_data()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient(new()
        {
            AllowAutoRedirect = true,
            HandleCookies = true
        });
        client.DefaultRequestHeaders.Add("Cookie", "bookkeeping.ui.language=en");
        string accountName = $"Travel Wallet {Guid.NewGuid():N}";
        string page = await GetSuccessfulHtmlAsync(client, "/Accounts");
        string token = ExtractRequestVerificationToken(page);

        HttpResponseMessage response = await client.PostAsync("/Accounts", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Input.Name"] = accountName,
            ["Input.Type"] = AccountType.Cash.ToString(),
            ["Input.OpeningBalance"] = "1000",
            ["Input.IconKey"] = "wallet",
            ["Input.DisplayOrder"] = "1"
        }));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.True(response.IsSuccessStatusCode, html);
        Assert.Contains("Account created.", html, StringComparison.Ordinal);
        Assert.Contains(accountName, html, StringComparison.Ordinal);
        Assert.DoesNotContain("帳戶已新增。", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task English_mode_localizes_system_labels_and_default_categories_without_translating_custom_categories()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        await SeedCustomCategoryAndAccountAsync(factory);
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", "bookkeeping.ui.language=en");

        string categories = await GetSuccessfulHtmlAsync(client, "/Categories");
        string accounts = await GetSuccessfulHtmlAsync(client, "/Accounts");

        Assert.Contains("Food & Dining", categories, StringComparison.Ordinal);
        Assert.Contains("Expense", categories, StringComparison.Ordinal);
        Assert.Contains("Pet Supplies", categories, StringComparison.Ordinal);
        Assert.Contains("Custom", categories, StringComparison.Ordinal);
        Assert.Contains("Bank", accounts, StringComparison.Ordinal);
        Assert.Contains("測試銀行", accounts, StringComparison.Ordinal);
    }

    private static async Task<string> GetSuccessfulHtmlAsync(HttpClient client, string route)
    {
        HttpResponseMessage response = await client.GetAsync(route);
        string body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);
        return WebUtility.HtmlDecode(body);
    }

    private static string ExtractRequestVerificationToken(string html)
    {
        Match match = AntiforgeryTokenRegex().Match(html);
        Assert.True(match.Success, "Antiforgery token should be rendered.");
        return WebUtility.HtmlDecode(match.Groups["token"].Value);
    }

    private static async Task SeedCustomCategoryAndAccountAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Categories.Add(new Category
        {
            Name = "Pet Supplies",
            NormalizedName = "PET SUPPLIES",
            Type = TransactionType.Expense,
            IconKey = "tag",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });
        context.Accounts.Add(new Account
        {
            Name = "測試銀行",
            NormalizedName = "測試銀行",
            Type = AccountType.Bank,
            IconKey = "bank",
            Currency = "TWD",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();
    }

    private static Regex LanguageRadioRegex(string value, bool requiredChecked)
    {
        string checkedLookahead = requiredChecked ? "(?=[^>]*\\bchecked)" : string.Empty;
        return new Regex(
            $"""<input(?=[^>]*\btype="radio")(?=[^>]*\bname="uiLanguage")(?=[^>]*\bvalue="{Regex.Escape(value)}"){checkedLookahead}[^>]*>""",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"(?<token>[^\"]+)\"")]
    private static partial Regex AntiforgeryTokenRegex();
}
