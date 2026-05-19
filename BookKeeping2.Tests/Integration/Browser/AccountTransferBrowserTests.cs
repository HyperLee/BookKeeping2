using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.AccountTransfers;
using BookKeeping2.Models.Common;
using BookKeeping2.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Xunit;

namespace BookKeeping2.Tests.Integration.Browser;

public sealed class AccountTransferBrowserTests : IClassFixture<ThemeModeBrowserFixture>
{
    private readonly ThemeModeBrowserFixture fixture;

    public AccountTransferBrowserTests(ThemeModeBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Theory]
    [InlineData(390)]
    [InlineData(1280)]
    public async Task Transfer_create_form_is_labeled_focusable_and_has_no_horizontal_overflow(int width)
    {
        await using BookKeepingWebApplicationFactory factory = new();
        await SeedAccountsAsync(factory);
        IPage page = await fixture.NewPageAsync(factory);

        await page.SetViewportSizeAsync(width, 900);
        await page.GotoAsync("/Transfers/Create");

        await Assertions.Expect(page.GetByLabel("轉帳日期")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByLabel("幣別")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByLabel("金額")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByLabel("轉出帳戶")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByLabel("轉入帳戶")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "儲存" })).ToBeVisibleAsync();
        await AssertNoHorizontalOverflowAsync(page, $"Transfer create form should not overflow at {width}px.");

        await page.Keyboard.PressAsync("Tab");
        await Assertions.Expect(page.Locator(":focus")).ToBeVisibleAsync();
        await page.CloseAsync();
    }

    [Theory]
    [InlineData(390)]
    [InlineData(1280)]
    public async Task Timeline_transfer_rows_show_direction_actions_and_remain_responsive(int width)
    {
        await using BookKeepingWebApplicationFactory factory = new();
        await SeedTransferAsync(factory);
        IPage page = await fixture.NewPageAsync(factory);

        await page.SetViewportSizeAsync(width, 900);
        await page.GotoAsync("/Transactions");

        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "新增轉帳" })).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByText("轉帳", new() { Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByText("銀行 -> 現金")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "編輯" }).First).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "刪除" }).First).ToBeVisibleAsync();
        await AssertNoHorizontalOverflowAsync(page, $"Transfer timeline should not overflow at {width}px.");
        await page.CloseAsync();
    }

    [Theory]
    [InlineData("/Csv/Import", 390)]
    [InlineData("/Csv/Import", 1280)]
    [InlineData("/Csv/Export", 390)]
    [InlineData("/Csv/Export", 1280)]
    public async Task Transfer_csv_surfaces_show_contract_and_remain_responsive(string path, int width)
    {
        await using BookKeepingWebApplicationFactory factory = new();
        IPage page = await fixture.NewPageAsync(factory);

        await page.SetViewportSizeAsync(width, 900);
        await page.GotoAsync(path);

        await Assertions.Expect(page.GetByText("日期,幣別,金額,轉出帳戶,轉入帳戶,備註")).ToBeVisibleAsync();
        await AssertNoHorizontalOverflowAsync(page, $"Transfer CSV page {path} should not overflow at {width}px.");
        await page.CloseAsync();
    }

    private static async Task AssertNoHorizontalOverflowAsync(IPage page, string message)
    {
        double scrollWidth = await page.EvaluateAsync<double>("document.documentElement.scrollWidth");
        double clientWidth = await page.EvaluateAsync<double>("document.documentElement.clientWidth");
        Assert.True(scrollWidth <= clientWidth + 1, message);
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
}
