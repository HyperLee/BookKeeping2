using BookKeeping2.Tests.TestSupport;
using Microsoft.Playwright;
using Xunit;

namespace BookKeeping2.Tests.Integration.Browser;

public sealed class MultiCurrencyBrowserTests : IClassFixture<ThemeModeBrowserFixture>
{
    private readonly ThemeModeBrowserFixture fixture;

    public MultiCurrencyBrowserTests(ThemeModeBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Theory]
    [InlineData(390)]
    [InlineData(1280)]
    public async Task Transaction_create_form_renders_currency_control_without_horizontal_overflow(int width)
    {
        await using BookKeepingWebApplicationFactory factory = new();
        IPage page = await fixture.NewPageAsync(factory);

        await page.SetViewportSizeAsync(width, 900);
        await page.GotoAsync("/Transactions/Create");

        await Assertions.Expect(page.GetByLabel("幣別")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("select[name='Input.Currency']")).ToContainTextAsync("TWD");
        await Assertions.Expect(page.Locator("select[name='Input.Currency']")).ToContainTextAsync("USD");
        double scrollWidth = await page.EvaluateAsync<double>("document.documentElement.scrollWidth");
        double clientWidth = await page.EvaluateAsync<double>("document.documentElement.clientWidth");
        Assert.True(scrollWidth <= clientWidth + 1, $"Transaction form should not overflow horizontally at {width}px.");

        await page.CloseAsync();
    }

    [Theory]
    [InlineData(390)]
    [InlineData(1280)]
    public async Task Accounts_page_renders_currency_control_without_horizontal_overflow(int width)
    {
        await using BookKeepingWebApplicationFactory factory = new();
        IPage page = await fixture.NewPageAsync(factory);

        await page.SetViewportSizeAsync(width, 900);
        await page.GotoAsync("/Accounts");

        await Assertions.Expect(page.GetByLabel("幣別")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("select[name='Input.Currency']")).ToContainTextAsync("TWD");
        await Assertions.Expect(page.Locator("select[name='Input.Currency']")).ToContainTextAsync("USD");
        double scrollWidth = await page.EvaluateAsync<double>("document.documentElement.scrollWidth");
        double clientWidth = await page.EvaluateAsync<double>("document.documentElement.clientWidth");
        Assert.True(scrollWidth <= clientWidth + 1, $"Accounts page should not overflow horizontally at {width}px.");

        await page.CloseAsync();
    }

    [Theory]
    [InlineData(390)]
    [InlineData(1280)]
    public async Task Budgets_page_renders_currency_control_without_horizontal_overflow(int width)
    {
        await using BookKeepingWebApplicationFactory factory = new();
        IPage page = await fixture.NewPageAsync(factory);

        await page.SetViewportSizeAsync(width, 900);
        await page.GotoAsync("/Budgets?Month=2026-05");

        await Assertions.Expect(page.GetByLabel("幣別")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("select[name='Input.Currency']")).ToContainTextAsync("TWD");
        await Assertions.Expect(page.Locator("select[name='Input.Currency']")).ToContainTextAsync("USD");
        double scrollWidth = await page.EvaluateAsync<double>("document.documentElement.scrollWidth");
        double clientWidth = await page.EvaluateAsync<double>("document.documentElement.clientWidth");
        Assert.True(scrollWidth <= clientWidth + 1, $"Budgets page should not overflow horizontally at {width}px.");

        await page.CloseAsync();
    }

    [Theory]
    [InlineData("/Csv/Import", 390)]
    [InlineData("/Csv/Import", 1280)]
    [InlineData("/Csv/Export", 390)]
    [InlineData("/Csv/Export", 1280)]
    public async Task Csv_pages_show_currency_contract_without_horizontal_overflow(string path, int width)
    {
        await using BookKeepingWebApplicationFactory factory = new();
        IPage page = await fixture.NewPageAsync(factory);

        await page.SetViewportSizeAsync(width, 900);
        await page.GotoAsync(path);

        await Assertions.Expect(page.GetByText("日期,類型,幣別,金額,分類,帳戶,備註")).ToBeVisibleAsync();
        double scrollWidth = await page.EvaluateAsync<double>("document.documentElement.scrollWidth");
        double clientWidth = await page.EvaluateAsync<double>("document.documentElement.clientWidth");
        Assert.True(scrollWidth <= clientWidth + 1, $"CSV page {path} should not overflow horizontally at {width}px.");

        await page.CloseAsync();
    }
}
