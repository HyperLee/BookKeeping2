using System.Text;
using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Services.Csv;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookKeeping2.Tests.Unit.Csv;

public sealed class CsvExportSecurityTests
{
    [Fact]
    public async Task ExportAsync_escapes_special_characters_and_prefixes_formula_risk_text_fields()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        var account = new Account { Name = "@現金", NormalizedName = "@現金", Type = AccountType.Cash, IconKey = "wallet", Currency = "TWD", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        var category = new Category { Name = "+餐飲", NormalizedName = "+餐飲", Type = TransactionType.Expense, IconKey = "tag", CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        context.AddRange(account, category);
        await context.SaveChangesAsync();
        context.Transactions.AddRange(
            Create(account.Id, category.Id, "午餐, \"便當\"\r\n第二行"),
            Create(account.Id, category.Id, "=SUM(A1:A2)"));
        await context.SaveChangesAsync();
        var service = new CsvExportService(context, TestDataBuilder.CreateDateService());

        CsvExportResult result = await service.ExportAsync(new CsvExportOptions());
        string csv = Encoding.UTF8.GetString(result.Content);

        Assert.Contains("\"午餐, \"\"便當\"\"\r\n第二行\"", csv);
        Assert.Contains("'+餐飲", csv);
        Assert.Contains("'@現金", csv);
        Assert.Contains("'=SUM(A1:A2)", csv);
    }

    private static Transaction Create(long accountId, long categoryId, string note)
    {
        return new Transaction
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Type = TransactionType.Expense,
            TransactionDate = new DateOnly(2026, 4, 1),
            Amount = 100m,
            Note = note,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastChangeSummary = "測試"
        };
    }
}
