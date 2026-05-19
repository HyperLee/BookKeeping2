using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.AccountTransfers;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookKeeping2.Tests.Integration.Pages;

public sealed partial class CsvTransferPageTests
{
    [Fact]
    public async Task Import_page_accepts_separate_transfer_upload_without_changing_transaction_import()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient(new() { AllowAutoRedirect = false });
        await SeedAccountsAsync(factory);
        string page = await client.GetStringAsync("/Csv/Import");
        string token = ExtractRequestVerificationToken(page);
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(token), "__RequestVerificationToken");
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes("日期,幣別,金額,轉出帳戶,轉入帳戶,備註\r\n2026-05-01,TWD,1000,銀行,現金,提款"));
        file.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        form.Add(file, "TransferUpload", "transfers.csv");

        HttpResponseMessage response = await client.PostAsync("/Csv/Import?handler=Transfer", form);
        string body = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("轉帳匯入結果", body);
        Assert.Equal(1, await CountTransfersAsync(factory));
    }

    [Fact]
    public async Task Export_page_downloads_transfer_csv_with_download_headers()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient(new() { AllowAutoRedirect = false });
        await SeedTransferAsync(factory);

        HttpResponseMessage response = await client.GetAsync("/Csv/Export?handler=TransferDownload");
        string csv = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString());
        Assert.Contains("日期,幣別,金額,轉出帳戶,轉入帳戶,備註", csv);
        Assert.Contains("銀行", csv);
    }

    private static async Task SeedAccountsAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Accounts.AddRange(
            TestDataBuilder.CreateAccount("銀行", TestDataBuilder.TwdCurrency),
            TestDataBuilder.CreateAccount("現金", TestDataBuilder.TwdCurrency));
        await context.SaveChangesAsync();
    }

    private static async Task SeedTransferAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Account bank = TestDataBuilder.CreateAccount("銀行", TestDataBuilder.TwdCurrency);
        Account cash = TestDataBuilder.CreateAccount("現金", TestDataBuilder.TwdCurrency);
        context.Accounts.AddRange(bank, cash);
        await context.SaveChangesAsync();
        context.AccountTransfers.Add(new AccountTransfer
        {
            FromAccountId = bank.Id,
            ToAccountId = cash.Id,
            TransferDate = TestDataBuilder.DefaultToday,
            Currency = TestDataBuilder.TwdCurrency,
            Amount = 1000m,
            SubmissionToken = Guid.NewGuid().ToString("N"),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "測試"
        });
        await context.SaveChangesAsync();
    }

    private static async Task<int> CountTransfersAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.AccountTransfers.CountAsync();
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
