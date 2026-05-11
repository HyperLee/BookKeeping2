using System.Net;
using System.Text.RegularExpressions;
using BookKeeping2.Data;
using BookKeeping2.Models.Common;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookKeeping2.Tests.Integration.Pages;

public sealed partial class BudgetsPageTests
{
    [Fact]
    public async Task User_can_create_monthly_budget_from_management_page()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient(new() { AllowAutoRedirect = false });
        long foodCategoryId = await GetFoodCategoryIdAsync(factory);

        string budgetsPage = await client.GetStringAsync("/Budgets?Month=2026-05");
        string token = ExtractRequestVerificationToken(budgetsPage);
        HttpResponseMessage response = await client.PostAsync("/Budgets", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Input.CategoryId"] = foodCategoryId.ToString(),
            ["Input.BudgetMonth"] = "2026-05-01",
            ["Input.Amount"] = "5000"
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        string updatedPage = WebUtility.HtmlDecode(await client.GetStringAsync("/Budgets?Month=2026-05"));
        Assert.Contains("預算管理", updatedPage);
        Assert.Contains("餐飲", updatedPage);
        Assert.Contains("5,000", updatedPage);
    }

    private static async Task<long> GetFoodCategoryIdAsync(BookKeepingWebApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.Categories
            .Where(category => category.Type == TransactionType.Expense && category.Name == "餐飲")
            .Select(category => category.Id)
            .SingleAsync();
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
