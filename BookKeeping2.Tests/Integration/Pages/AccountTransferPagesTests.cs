using System.Net;
using System.Text.RegularExpressions;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.AccountTransfers;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookKeeping2.Tests.Integration.Pages;

public sealed partial class AccountTransferPagesTests
{
    [Fact]
    public async Task User_can_create_edit_and_soft_delete_transfer_from_pages()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient(new() { AllowAutoRedirect = false });
        (long bankId, long cashId, long pettyCashId) = await SeedAccountsAsync(factory);

        string createPage = await GetSuccessfulStringAsync(client, "/Transfers/Create");
        Assert.Contains("SubmissionToken", createPage);
        string createToken = ExtractRequestVerificationToken(createPage);
        string submissionToken = ExtractSubmissionToken(createPage);

        HttpResponseMessage createResponse = await client.PostAsync("/Transfers/Create", FormContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = createToken,
            ["Input.TransferDate"] = TestDataBuilder.DefaultToday.ToString("yyyy-MM-dd"),
            ["Input.Currency"] = TestDataBuilder.TwdCurrency,
            ["Input.Amount"] = "1000",
            ["Input.FromAccountId"] = bankId.ToString(),
            ["Input.ToAccountId"] = cashId.ToString(),
            ["Input.Note"] = "提款轉現金",
            ["Input.SubmissionToken"] = submissionToken
        }));

        await AssertStatusCodeAsync(HttpStatusCode.Redirect, createResponse);
        long transferId = await GetOnlyTransferIdAsync(factory);

        string editPage = WebUtility.HtmlDecode(await GetSuccessfulStringAsync(client, $"/Transfers/Edit/{transferId}"));
        Assert.Contains("提款轉現金", editPage);
        string editToken = ExtractRequestVerificationToken(editPage);
        HttpResponseMessage editResponse = await client.PostAsync($"/Transfers/Edit/{transferId}", FormContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = editToken,
            ["Input.TransferDate"] = TestDataBuilder.DefaultToday.AddDays(-1).ToString("yyyy-MM-dd"),
            ["Input.Currency"] = TestDataBuilder.TwdCurrency,
            ["Input.Amount"] = "250",
            ["Input.FromAccountId"] = bankId.ToString(),
            ["Input.ToAccountId"] = pettyCashId.ToString(),
            ["Input.Note"] = "更新轉帳"
        }));

        await AssertStatusCodeAsync(HttpStatusCode.Redirect, editResponse);
        AccountTransfer edited = await GetOnlyTransferAsync(factory);
        Assert.Equal(250m, edited.Amount);
        Assert.Equal(pettyCashId, edited.ToAccountId);

        string deletePage = WebUtility.HtmlDecode(await GetSuccessfulStringAsync(client, $"/Transfers/Delete/{transferId}"));
        Assert.Contains("銀行 -> 零用金", deletePage);
        string deleteToken = ExtractRequestVerificationToken(deletePage);
        HttpResponseMessage deleteResponse = await client.PostAsync($"/Transfers/Delete/{transferId}", FormContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = deleteToken
        }));

        await AssertStatusCodeAsync(HttpStatusCode.Redirect, deleteResponse);
        Assert.True((await GetOnlyTransferAsync(factory)).IsDeleted);
        HttpResponseMessage deletedEdit = await client.GetAsync($"/Transfers/Edit/{transferId}");
        Assert.Equal(HttpStatusCode.NotFound, deletedEdit.StatusCode);
    }

    [Fact]
    public async Task Create_post_requires_antiforgery_and_preserves_validation_state()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient(new() { AllowAutoRedirect = false });
        (long bankId, _, _) = await SeedAccountsAsync(factory);

        HttpResponseMessage missingAntiforgery = await client.PostAsync("/Transfers/Create", FormContent(new Dictionary<string, string>
        {
            ["Input.TransferDate"] = TestDataBuilder.DefaultToday.ToString("yyyy-MM-dd"),
            ["Input.Currency"] = TestDataBuilder.TwdCurrency,
            ["Input.Amount"] = "1000",
            ["Input.FromAccountId"] = bankId.ToString(),
            ["Input.ToAccountId"] = bankId.ToString(),
            ["Input.SubmissionToken"] = TestDataBuilder.CreateTransferSubmissionToken()
        }));
        Assert.Equal(HttpStatusCode.BadRequest, missingAntiforgery.StatusCode);

        string createPage = await GetSuccessfulStringAsync(client, "/Transfers/Create");
        string token = ExtractRequestVerificationToken(createPage);
        string submissionToken = ExtractSubmissionToken(createPage);
        HttpResponseMessage validationResponse = await client.PostAsync("/Transfers/Create", FormContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Input.TransferDate"] = TestDataBuilder.DefaultToday.ToString("yyyy-MM-dd"),
            ["Input.Currency"] = TestDataBuilder.TwdCurrency,
            ["Input.Amount"] = "1000",
            ["Input.FromAccountId"] = bankId.ToString(),
            ["Input.ToAccountId"] = bankId.ToString(),
            ["Input.Note"] = "保留欄位",
            ["Input.SubmissionToken"] = submissionToken
        }));

        string body = WebUtility.HtmlDecode(await validationResponse.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, validationResponse.StatusCode);
        Assert.Contains("轉出帳戶與轉入帳戶不可相同", body);
        Assert.Contains("保留欄位", body);
        Assert.Equal(0, await CountTransfersAsync(factory));
    }

    [Fact]
    public async Task Duplicate_submission_token_does_not_create_second_transfer()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient(new() { AllowAutoRedirect = false });
        (long bankId, long cashId, _) = await SeedAccountsAsync(factory);
        string createPage = await GetSuccessfulStringAsync(client, "/Transfers/Create");
        string token = ExtractRequestVerificationToken(createPage);
        string submissionToken = ExtractSubmissionToken(createPage);
        var values = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Input.TransferDate"] = TestDataBuilder.DefaultToday.ToString("yyyy-MM-dd"),
            ["Input.Currency"] = TestDataBuilder.TwdCurrency,
            ["Input.Amount"] = "1000",
            ["Input.FromAccountId"] = bankId.ToString(),
            ["Input.ToAccountId"] = cashId.ToString(),
            ["Input.Note"] = "重複送出",
            ["Input.SubmissionToken"] = submissionToken
        };

        HttpResponseMessage first = await client.PostAsync("/Transfers/Create", FormContent(values));
        HttpResponseMessage second = await client.PostAsync("/Transfers/Create", FormContent(values));

        await AssertStatusCodeAsync(HttpStatusCode.Redirect, first);
        await AssertStatusCodeAsync(HttpStatusCode.Redirect, second);
        Assert.Equal(1, await CountTransfersAsync(factory));
    }

    private static async Task<(long BankId, long CashId, long PettyCashId)> SeedAccountsAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Account bank = TestDataBuilder.CreateAccount("銀行", TestDataBuilder.TwdCurrency);
        bank.OpeningBalance = 500m;
        Account cash = TestDataBuilder.CreateAccount("現金", TestDataBuilder.TwdCurrency);
        Account pettyCash = TestDataBuilder.CreateAccount("零用金", TestDataBuilder.TwdCurrency);
        context.Accounts.AddRange(bank, cash, pettyCash);
        await context.SaveChangesAsync();
        return (bank.Id, cash.Id, pettyCash.Id);
    }

    private static async Task<long> GetOnlyTransferIdAsync(BookKeepingWebApplicationFactory factory)
    {
        return (await GetOnlyTransferAsync(factory)).Id;
    }

    private static async Task<AccountTransfer> GetOnlyTransferAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.AccountTransfers.SingleAsync();
    }

    private static async Task<int> CountTransfersAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.AccountTransfers.CountAsync();
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

    private static string ExtractSubmissionToken(string html)
    {
        Match match = SubmissionTokenRegex().Match(html);
        Assert.True(match.Success, "SubmissionToken should be rendered.");
        return WebUtility.HtmlDecode(match.Groups["token"].Value);
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"(?<token>[^\"]+)\"")]
    private static partial Regex AntiforgeryTokenRegex();

    [GeneratedRegex("name=\"Input.SubmissionToken\"[^>]*value=\"(?<token>[^\"]+)\"")]
    private static partial Regex SubmissionTokenRegex();
}
