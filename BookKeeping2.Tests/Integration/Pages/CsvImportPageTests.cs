using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Common;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookKeeping2.Tests.Integration.Pages;

public sealed partial class CsvImportPageTests
{
    [Fact]
    public async Task Csv_import_page_uploads_file_and_shows_error_summary_with_antiforgery()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient(new() { AllowAutoRedirect = false });
        await SeedAccountAsync(factory);

        string page = await client.GetStringAsync("/Csv/Import");
        string token = ExtractRequestVerificationToken(page);
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(token), "__RequestVerificationToken");
        string csv = "日期,類型,金額,分類,帳戶,備註\r\n"
            + "2026-02-01,支出,150,餐飲,現金,午餐\r\n"
            + "2026-02-02,支出,100,餐飲,不存在,錯誤";
        form.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(csv)), "Upload", "transactions.csv");

        HttpResponseMessage response = await client.PostAsync("/Csv/Import", form);
        string body = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("CSV 匯入", body);
        Assert.Contains("成功 1 筆", body);
        Assert.Contains("失敗 1 筆", body);
        Assert.Contains("第 3 行匯入失敗", body);

        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(1, await context.Transactions.CountAsync());
    }

    private static async Task SeedAccountAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Accounts.Add(new Account
        {
            Name = "現金",
            NormalizedName = "現金",
            Type = AccountType.Cash,
            IconKey = "wallet",
            Currency = "TWD",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();
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
