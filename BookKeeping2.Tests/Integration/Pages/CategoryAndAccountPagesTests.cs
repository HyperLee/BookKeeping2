using System.Net;
using System.Text.RegularExpressions;
using BookKeeping2.Tests.TestSupport;
using Xunit;

namespace BookKeeping2.Tests.Integration.Pages;

public sealed partial class CategoryAndAccountPagesTests
{
    [Fact]
    public async Task User_can_create_category_and_account_from_management_pages()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient(new() { AllowAutoRedirect = false });

        string categoriesPage = await client.GetStringAsync("/Categories");
        string categoryToken = ExtractRequestVerificationToken(categoriesPage);
        HttpResponseMessage categoryResponse = await client.PostAsync("/Categories", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = categoryToken,
            ["Input.Name"] = "寵物",
            ["Input.Type"] = "Expense",
            ["Input.IconKey"] = "tag",
            ["Input.DisplayOrder"] = "20"
        }));

        Assert.Equal(HttpStatusCode.Redirect, categoryResponse.StatusCode);
        Assert.Contains("寵物", WebUtility.HtmlDecode(await client.GetStringAsync("/Categories")));

        string accountsPage = await client.GetStringAsync("/Accounts");
        string accountToken = ExtractRequestVerificationToken(accountsPage);
        HttpResponseMessage accountResponse = await client.PostAsync("/Accounts", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = accountToken,
            ["Input.Name"] = "銀行",
            ["Input.Type"] = "Bank",
            ["Input.OpeningBalance"] = "1000",
            ["Input.IconKey"] = "bank",
            ["Input.DisplayOrder"] = "1"
        }));

        Assert.Equal(HttpStatusCode.Redirect, accountResponse.StatusCode);
        Assert.Contains("銀行", WebUtility.HtmlDecode(await client.GetStringAsync("/Accounts")));
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
