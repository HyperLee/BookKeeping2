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

namespace BookKeeping2.Tests.Integration.Persistence;

public sealed partial class CrossBrowserConsistencyTests
{
    [Fact]
    public async Task Two_clients_share_the_same_single_site_ledger_without_login_or_roles()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient firstClient = factory.CreateClient(new() { AllowAutoRedirect = false });
        HttpClient secondClient = factory.CreateClient(new() { AllowAutoRedirect = false });
        (long categoryId, long accountId) = await SeedAccountAndGetExpenseCategoryAsync(factory);

        string createPage = await firstClient.GetStringAsync("/Transactions/Create");
        string token = ExtractRequestVerificationToken(createPage);

        HttpResponseMessage response = await firstClient.PostAsync("/Transactions/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Input.TransactionDate"] = TestDataBuilder.DefaultToday.ToString("yyyy-MM-dd"),
            ["Input.Type"] = TransactionType.Expense.ToString(),
            ["Input.Amount"] = "150",
            ["Input.CategoryId"] = categoryId.ToString(),
            ["Input.AccountId"] = accountId.ToString(),
            ["Input.Note"] = "第二個瀏覽器可見"
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        string listPage = WebUtility.HtmlDecode(await secondClient.GetStringAsync("/Transactions"));

        Assert.Contains("第二個瀏覽器可見", listPage);
        Assert.DoesNotContain("登入", listPage);
        Assert.DoesNotContain("角色", listPage);
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

    private static string ExtractRequestVerificationToken(string html)
    {
        Match match = AntiforgeryTokenRegex().Match(html);
        Assert.True(match.Success, "Antiforgery token should be rendered.");
        return WebUtility.HtmlDecode(match.Groups["token"].Value);
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"(?<token>[^\"]+)\"")]
    private static partial Regex AntiforgeryTokenRegex();
}
