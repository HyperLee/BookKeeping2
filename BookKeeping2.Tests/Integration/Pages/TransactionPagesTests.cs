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

public sealed partial class TransactionPagesTests
{
    [Fact]
    public async Task User_can_create_edit_and_soft_delete_transaction_from_pages()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient(new() { AllowAutoRedirect = false });
        (long categoryId, long accountId) = await SeedAccountAndGetExpenseCategoryAsync(factory);

        string createPage = await GetSuccessfulStringAsync(client, "/Transactions/Create");
        string createToken = ExtractRequestVerificationToken(createPage);

        HttpResponseMessage createResponse = await client.PostAsync("/Transactions/Create", FormContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = createToken,
            ["Input.TransactionDate"] = TestDataBuilder.DefaultToday.ToString("yyyy-MM-dd"),
            ["Input.Type"] = TransactionType.Expense.ToString(),
            ["Input.Amount"] = "150",
            ["Input.CategoryId"] = categoryId.ToString(),
            ["Input.AccountId"] = accountId.ToString(),
            ["Input.Note"] = "午餐便當"
        }));

        await AssertStatusCodeAsync(HttpStatusCode.Redirect, createResponse);
        Assert.Equal(1, await CountTransactionsAsync(factory));

        string listPage = WebUtility.HtmlDecode(await GetSuccessfulStringAsync(client, "/Transactions"));
        Assert.True(listPage.Contains("餐飲", StringComparison.Ordinal), listPage);
        Assert.Contains("150", listPage);
        Assert.Contains("午餐便當", listPage);

        string homePage = WebUtility.HtmlDecode(await GetSuccessfulStringAsync(client, "/"));
        Assert.Contains("150", homePage);

        long transactionId = await GetOnlyTransactionIdAsync(factory);
        string editPage = await GetSuccessfulStringAsync(client, $"/Transactions/Edit/{transactionId}");
        string editToken = ExtractRequestVerificationToken(editPage);

        HttpResponseMessage editResponse = await client.PostAsync($"/Transactions/Edit/{transactionId}", FormContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = editToken,
            ["Input.TransactionDate"] = TestDataBuilder.DefaultToday.ToString("yyyy-MM-dd"),
            ["Input.Type"] = TransactionType.Expense.ToString(),
            ["Input.Amount"] = "200",
            ["Input.CategoryId"] = categoryId.ToString(),
            ["Input.AccountId"] = accountId.ToString(),
            ["Input.Note"] = "晚餐便當"
        }));

        await AssertStatusCodeAsync(HttpStatusCode.Redirect, editResponse);
        listPage = WebUtility.HtmlDecode(await GetSuccessfulStringAsync(client, "/Transactions"));
        Assert.Contains("200", listPage);
        Assert.Contains("晚餐便當", listPage);

        string deletePage = await GetSuccessfulStringAsync(client, $"/Transactions/Delete/{transactionId}");
        string deleteToken = ExtractRequestVerificationToken(deletePage);
        HttpResponseMessage deleteResponse = await client.PostAsync($"/Transactions/Delete/{transactionId}", FormContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = deleteToken
        }));

        await AssertStatusCodeAsync(HttpStatusCode.Redirect, deleteResponse);
        listPage = WebUtility.HtmlDecode(await GetSuccessfulStringAsync(client, "/Transactions"));
        Assert.DoesNotContain("晚餐便當", listPage);

        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.True(await context.Transactions.IgnoreQueryFilters().AnyAsync(transaction => transaction.Id == transactionId && transaction.IsDeleted));
        Assert.True(await context.AuditEvents.AnyAsync());
    }

    private static async Task<(long CategoryId, long AccountId)> SeedAccountAndGetExpenseCategoryAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Category category = await context.Categories.FirstAsync(category => category.Type == TransactionType.Expense && category.Name == "餐飲");
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
        return (category.Id, account.Id);
    }

    private static async Task<long> GetOnlyTransactionIdAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.Transactions.Select(transaction => transaction.Id).SingleAsync();
    }

    private static async Task<int> CountTransactionsAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.Transactions.CountAsync();
    }

    private static FormUrlEncodedContent FormContent(Dictionary<string, string> values)
    {
        return new FormUrlEncodedContent(values);
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

    private static string ExtractRequestVerificationToken(string html)
    {
        Match match = AntiforgeryTokenRegex().Match(html);
        Assert.True(match.Success, "Antiforgery token should be rendered.");
        return WebUtility.HtmlDecode(match.Groups["token"].Value);
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"(?<token>[^\"]+)\"")]
    private static partial Regex AntiforgeryTokenRegex();
}
