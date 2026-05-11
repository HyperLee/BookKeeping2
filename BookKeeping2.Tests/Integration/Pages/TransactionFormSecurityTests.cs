using System.Net;
using System.Text.RegularExpressions;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookKeeping2.Tests.Integration.Pages;

public sealed partial class TransactionFormSecurityTests
{
    [Fact]
    public async Task Create_post_requires_antiforgery_token()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient(new() { AllowAutoRedirect = false });
        (long categoryId, long accountId) = await SeedAccountAndGetExpenseCategoryAsync(factory);

        HttpResponseMessage response = await client.PostAsync("/Transactions/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.TransactionDate"] = TestDataBuilder.DefaultToday.ToString("yyyy-MM-dd"),
            ["Input.Type"] = TransactionType.Expense.ToString(),
            ["Input.Amount"] = "150",
            ["Input.CategoryId"] = categoryId.ToString(),
            ["Input.AccountId"] = accountId.ToString()
        }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Duplicate_create_submission_does_not_create_second_transaction()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient(new() { AllowAutoRedirect = false });
        (long categoryId, long accountId) = await SeedAccountAndGetExpenseCategoryAsync(factory);
        string createPage = await GetSuccessfulStringAsync(client, "/Transactions/Create");
        string token = ExtractRequestVerificationToken(createPage);
        var values = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Input.TransactionDate"] = TestDataBuilder.DefaultToday.ToString("yyyy-MM-dd"),
            ["Input.Type"] = TransactionType.Expense.ToString(),
            ["Input.Amount"] = "150",
            ["Input.CategoryId"] = categoryId.ToString(),
            ["Input.AccountId"] = accountId.ToString(),
            ["Input.Note"] = "重複送出測試"
        };

        HttpResponseMessage first = await client.PostAsync("/Transactions/Create", new FormUrlEncodedContent(values));
        HttpResponseMessage second = await client.PostAsync("/Transactions/Create", new FormUrlEncodedContent(values));

        await AssertStatusCodeAsync(HttpStatusCode.Redirect, first);
        await AssertStatusCodeAsync(HttpStatusCode.Redirect, second);

        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(1, await context.Transactions.CountAsync());
    }

    private static async Task<(long CategoryId, long AccountId)> SeedAccountAndGetExpenseCategoryAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Category category = await context.Categories.FirstAsync(category => category.Type == TransactionType.Expense && category.Name == "餐飲");
        var account = new Account
        {
            Name = Guid.NewGuid().ToString("N"),
            NormalizedName = Guid.NewGuid().ToString("N"),
            Type = AccountType.Cash,
            IconKey = "wallet",
            Currency = "TWD",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        return (category.Id, account.Id);
    }

    private static string ExtractRequestVerificationToken(string html)
    {
        Match match = AntiforgeryTokenRegex().Match(html);
        Assert.True(match.Success, "Antiforgery token should be rendered.");
        return WebUtility.HtmlDecode(match.Groups["token"].Value);
    }

    private static async Task<string> GetSuccessfulStringAsync(HttpClient client, string requestUri)
    {
        HttpResponseMessage response = await client.GetAsync(requestUri);
        string body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);
        return body;
    }

    private static async Task AssertStatusCodeAsync(HttpStatusCode expected, HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == expected, body);
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"(?<token>[^\"]+)\"")]
    private static partial Regex AntiforgeryTokenRegex();
}
