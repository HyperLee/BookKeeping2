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
}
