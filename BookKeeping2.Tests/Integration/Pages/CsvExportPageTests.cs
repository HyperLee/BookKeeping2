using System.Net;
using System.Text;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Audit;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookKeeping2.Tests.Integration.Pages;

public sealed class CsvExportPageTests
{
    [Fact]
    public async Task Csv_export_page_downloads_no_store_csv_and_records_audit_event()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();
        await SeedTransactionAsync(factory);

        string page = WebUtility.HtmlDecode(await client.GetStringAsync("/Csv/Export"));
        Assert.Contains("CSV 匯出", page);

        HttpResponseMessage response = await client.GetAsync("/Csv/Export?handler=Download&StartDate=2026-02-01&EndDate=2026-02-28");
        byte[] content = await response.Content.ReadAsByteArrayAsync();
        string csv = Encoding.UTF8.GetString(content);

        Assert.True(response.IsSuccessStatusCode, csv);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("utf-8", response.Content.Headers.ContentType?.CharSet);
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString());
        Assert.Contains("transactions-", response.Content.Headers.ContentDisposition?.FileNameStar ?? response.Content.Headers.ContentDisposition?.FileName);
        Assert.Contains("日期,類型,幣別,金額,分類,帳戶,備註", csv);
        Assert.Contains("2026-02-10,支出,TWD,150,餐飲,現金,午餐", csv);

        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.True(await context.AuditEvents.AnyAsync(audit => audit.EventType == AuditEventType.CsvExported));
    }

    [Fact]
    public async Task English_mode_csv_export_keeps_fixed_traditional_chinese_file_contract_and_raw_values()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", "bookkeeping.ui.language=en");
        await SeedTransactionAsync(factory);

        string page = WebUtility.HtmlDecode(await client.GetStringAsync("/Csv/Export"));
        Assert.Contains("CSV Export", page, StringComparison.Ordinal);

        HttpResponseMessage response = await client.GetAsync("/Csv/Export?handler=Download&StartDate=2026-02-01&EndDate=2026-02-28");
        string csv = Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync());

        Assert.True(response.IsSuccessStatusCode, csv);
        Assert.Contains("日期,類型,幣別,金額,分類,帳戶,備註", csv, StringComparison.Ordinal);
        Assert.Contains("2026-02-10,支出,TWD,150,餐飲,現金,午餐", csv, StringComparison.Ordinal);
        Assert.DoesNotContain("Date,Type,Amount,Category,Account,Note", csv, StringComparison.Ordinal);
        Assert.DoesNotContain("Expense", csv, StringComparison.Ordinal);
    }

    private static async Task SeedTransactionAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Category food = await context.Categories.FirstAsync(category => category.Type == TransactionType.Expense && category.Name == "餐飲");
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
        context.Transactions.Add(new Transaction
        {
            AccountId = account.Id,
            CategoryId = food.Id,
            Type = TransactionType.Expense,
            TransactionDate = new DateOnly(2026, 2, 10),
            Amount = 150m,
            Note = "午餐",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "測試"
        });
        await context.SaveChangesAsync();
    }
}
