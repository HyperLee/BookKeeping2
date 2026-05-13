using BookKeeping2.Tests.TestSupport;
using Microsoft.Playwright;
using Xunit;

namespace BookKeeping2.Tests.Integration.Browser;

public sealed class LanguageToggleBrowserTests : IClassFixture<LanguageToggleBrowserFixture>
{
    private readonly LanguageToggleBrowserFixture fixture;

    public LanguageToggleBrowserTests(LanguageToggleBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task English_cookie_renders_selected_language_and_navigation_within_one_second()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        IBrowserContext context = await fixture.NewContextAsync(factory);
        await context.AddCookiesAsync(
        [
            new()
            {
                Name = "bookkeeping.ui.language",
                Value = "en",
                Domain = "bookkeeping-language.test",
                Path = "/"
            }
        ]);
        IPage page = await context.NewPageAsync();

        await page.GotoAsync("/");
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("lang", "en", new() { Timeout = 1_000 });
        await Assertions.Expect(page.GetByText("Monthly Income")).ToBeVisibleAsync(new() { Timeout = 1_000 });

        await page.GotoAsync("/Transactions");

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Transaction Details" })).ToBeVisibleAsync(new() { Timeout = 1_000 });
        await context.CloseAsync();
    }

    [Fact]
    public async Task Homepage_language_options_are_keyboard_selectable()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        IPage page = await fixture.NewPageAsync(factory);

        await page.GotoAsync("/");
        ILocator englishOption = page.GetByLabel("English");
        await englishOption.FocusAsync();
        await page.Keyboard.PressAsync("Space");

        await Assertions.Expect(englishOption).ToBeCheckedAsync();
        await page.CloseAsync();
    }

    [Fact]
    public async Task Saved_english_language_survives_reload_and_return_visit_with_checked_option()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        IBrowserContext context = await fixture.NewContextAsync(factory);
        await context.AddCookiesAsync(
        [
            new()
            {
                Name = "bookkeeping.ui.language",
                Value = "en",
                Domain = "bookkeeping-language.test",
                Path = "/"
            }
        ]);
        IPage page = await context.NewPageAsync();

        await page.GotoAsync("/");
        await page.ReloadAsync();

        await Assertions.Expect(page.GetByLabel("English")).ToBeCheckedAsync();
        await Assertions.Expect(page.GetByText("Monthly Income")).ToBeVisibleAsync(new() { Timeout = 1_000 });

        IPage returnPage = await context.NewPageAsync();
        await returnPage.GotoAsync("/");
        await Assertions.Expect(returnPage.GetByLabel("English")).ToBeCheckedAsync();
        await Assertions.Expect(returnPage.GetByText("Monthly Income")).ToBeVisibleAsync(new() { Timeout = 1_000 });
        await context.CloseAsync();
    }

    [Fact]
    public async Task Language_control_theme_control_and_long_english_text_remain_responsive_and_focusable()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        IBrowserContext context = await fixture.NewContextAsync(factory);
        await context.AddCookiesAsync(
        [
            new()
            {
                Name = "bookkeeping.ui.language",
                Value = "en",
                Domain = "bookkeeping-language.test",
                Path = "/"
            }
        ]);
        IPage page = await context.NewPageAsync();

        foreach (int width in new[] { 390, 768, 1280 })
        {
            await page.SetViewportSizeAsync(width, 900);
            await page.GotoAsync("/");

            await Assertions.Expect(page.Locator("[data-language-control]")).ToBeVisibleAsync();
            await Assertions.Expect(page.Locator("[data-theme-mode-control]")).ToBeVisibleAsync();
            await page.GetByLabel("Dark Mode").CheckAsync();
            await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-bs-theme", "dark", new() { Timeout = 1_000 });

            double scrollWidth = await page.EvaluateAsync<double>("document.documentElement.scrollWidth");
            double clientWidth = await page.EvaluateAsync<double>("document.documentElement.clientWidth");
            Assert.True(scrollWidth <= clientWidth + 1, $"Language layout should not overflow horizontally at {width}px.");

            await page.GetByLabel("English").FocusAsync();
            await Assertions.Expect(page.Locator(":focus")).ToBeVisibleAsync();
        }

        await context.CloseAsync();
    }
}
