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

    [Fact]
    public async Task Primary_multi_currency_flow_creates_account_transaction_and_home_summary_under_performance_targets()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        IPage page = await fixture.NewPageAsync(factory);

        await page.SetViewportSizeAsync(1280, 900);
        await page.GotoAsync("/Accounts");
        await page.Locator("input[name='Input.Name']").FillAsync("美元測試帳戶");
        await page.Locator("select[name='Input.Currency']").SelectOptionAsync(new[] { new SelectOptionValue { Value = "USD" } });
        await Assertions.Expect(page.Locator("input[name='Input.Name']")).ToHaveValueAsync("美元測試帳戶");
        await Assertions.Expect(page.Locator("select[name='Input.Currency']")).ToHaveValueAsync("USD");
        await page.Locator("form").EvaluateAsync("form => form.requestSubmit()");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Assertions.Expect(page.GetByText("美元測試帳戶")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByText("USD 0.00")).ToBeVisibleAsync();

        await page.GotoAsync("/Transactions/Create");
        await page.Locator("input[name='Input.TransactionDate']").FillAsync("2026-05-10");
        await page.Locator("select[name='Input.Currency']").SelectOptionAsync(new[] { new SelectOptionValue { Value = "USD" } });
        await page.Locator("input[name='Input.Amount']").FillAsync("123.45");
        await page.Locator("select[name='Input.CategoryId']").SelectOptionAsync(new[] { new SelectOptionValue { Label = "餐飲" } });
        await page.Locator("select[name='Input.AccountId']").SelectOptionAsync(new[] { new SelectOptionValue { Label = "美元測試帳戶 (USD)" } });
        await page.Locator("textarea[name='Input.Note']").FillAsync("美元主流程");
        await page.Locator("form").EvaluateAsync("form => form.requestSubmit()");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Assertions.Expect(page.GetByText("美元主流程")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByText("USD 123.45")).ToBeVisibleAsync();

        await page.GotoAsync("/");
        await Assertions.Expect(page.GetByText("USD 美金")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByText("USD 123.45").First).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByText("USD -123.45").First).ToBeVisibleAsync();

        double[] metrics = await page.EvaluateAsync<double[]>(
            @"() => {
                const navigation = performance.getEntriesByType('navigation')[0];
                const paints = performance.getEntriesByType('paint');
                const fcp = paints.find((entry) => entry.name === 'first-contentful-paint')?.startTime
                    ?? navigation.domContentLoadedEventEnd;
                const lcpEntries = performance.getEntriesByType('largest-contentful-paint');
                const lcp = lcpEntries.length > 0
                    ? lcpEntries[lcpEntries.length - 1].startTime
                    : navigation.loadEventEnd;
                return [fcp, lcp];
            }");
        Assert.True(metrics[0] < 1_500, $"Homepage FCP should be below 1.5 seconds, actual {metrics[0]} ms.");
        Assert.True(metrics[1] < 2_500, $"Homepage LCP should be below 2.5 seconds, actual {metrics[1]} ms.");

        await page.CloseAsync();
    }
}
