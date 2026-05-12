using BookKeeping2.Tests.TestSupport;
using Microsoft.Playwright;
using Xunit;

namespace BookKeeping2.Tests.Integration.Browser;

public sealed class ThemeModeBrowserTests : IClassFixture<ThemeModeBrowserFixture>
{
    private readonly ThemeModeBrowserFixture fixture;

    public ThemeModeBrowserTests(ThemeModeBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task Selected_homepage_theme_applies_after_navigation_within_one_second()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        IPage page = await fixture.NewPageAsync(factory);

        await page.GotoAsync("/");
        await page.GetByLabel("暗黑模式").CheckAsync();
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-bs-theme", "dark", new() { Timeout = 1_000 });

        await page.GotoAsync("/Privacy");

        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-bs-theme", "dark", new() { Timeout = 1_000 });
        await page.CloseAsync();
    }

    [Fact]
    public async Task System_mode_tracks_preferred_color_scheme_within_two_seconds()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        IPage page = await fixture.NewPageAsync(factory);

        await page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });
        await page.GotoAsync("/");

        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-theme-mode", "system", new() { Timeout = 2_000 });
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-bs-theme", "dark", new() { Timeout = 2_000 });

        await page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Light });

        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-bs-theme", "light", new() { Timeout = 2_000 });
        await page.CloseAsync();
    }

    [Fact]
    public async Task Explicit_light_and_dark_modes_ignore_system_preference_changes()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        IPage page = await fixture.NewPageAsync(factory);

        await page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Light });
        await page.GotoAsync("/");

        await page.GetByLabel("亮色模式").CheckAsync();
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-bs-theme", "light", new() { Timeout = 1_000 });

        await page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-bs-theme", "light", new() { Timeout = 2_000 });

        await page.GetByLabel("暗黑模式").CheckAsync();
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-bs-theme", "dark", new() { Timeout = 1_000 });

        await page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Light });
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-bs-theme", "dark", new() { Timeout = 2_000 });
        await page.CloseAsync();
    }

    [Fact]
    public async Task Invalid_saved_mode_falls_back_to_system_and_keeps_storage_key_scoped()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        IPage page = await fixture.NewPageAsync(factory);

        await page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });
        await page.GotoAsync("/");
        await page.EvaluateAsync("localStorage.setItem('bookkeeping.theme.mode', 'invalid')");
        await page.ReloadAsync();

        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-theme-mode", "system", new() { Timeout = 2_000 });
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-bs-theme", "dark", new() { Timeout = 2_000 });

        string? storedMode = await page.EvaluateAsync<string?>("localStorage.getItem('bookkeeping.theme.mode')");
        Assert.Equal("invalid", storedMode);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Theme_mode_changes_synchronize_across_same_origin_tabs_within_two_seconds()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        IBrowserContext context = await fixture.NewContextAsync(factory);
        IPage firstPage = await context.NewPageAsync();
        IPage secondPage = await context.NewPageAsync();

        await firstPage.GotoAsync("/");
        await secondPage.GotoAsync("/");

        await firstPage.GetByLabel("暗黑模式").CheckAsync();

        await Assertions.Expect(secondPage.Locator("html")).ToHaveAttributeAsync("data-bs-theme", "dark", new() { Timeout = 2_000 });
        await context.CloseAsync();
    }

    [Fact]
    public async Task Theme_switching_does_not_submit_forms_or_call_finance_endpoints()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        IPage page = await fixture.NewPageAsync(factory);
        List<string> unsafeRequests = [];
        page.Request += (_, request) =>
        {
            Uri uri = new(request.Url);
            if (uri.AbsolutePath.StartsWith("/Transactions", StringComparison.Ordinal)
                || uri.AbsolutePath.StartsWith("/Accounts", StringComparison.Ordinal)
                || uri.AbsolutePath.StartsWith("/Budgets", StringComparison.Ordinal)
                || uri.AbsolutePath.StartsWith("/Categories", StringComparison.Ordinal)
                || uri.AbsolutePath.StartsWith("/Csv", StringComparison.Ordinal)
                || uri.AbsolutePath.StartsWith("/Reports", StringComparison.Ordinal))
            {
                unsafeRequests.Add(request.Url);
            }
        };

        await page.GotoAsync("/");
        unsafeRequests.Clear();
        await page.GetByLabel("暗黑模式").CheckAsync();
        await page.GetByLabel("亮色模式").CheckAsync();
        await page.GetByLabel("跟隨系統").CheckAsync();

        Assert.Empty(unsafeRequests);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Theme_control_remains_readable_and_focusable_at_responsive_widths()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        IPage page = await fixture.NewPageAsync(factory);

        foreach (int width in new[] { 390, 768, 1280 })
        {
            await page.SetViewportSizeAsync(width, 900);
            await page.GotoAsync("/");

            ILocator control = page.Locator("[data-theme-mode-control]");
            await Assertions.Expect(control).ToBeVisibleAsync();
            double scrollWidth = await page.EvaluateAsync<double>("document.documentElement.scrollWidth");
            double clientWidth = await page.EvaluateAsync<double>("document.documentElement.clientWidth");
            Assert.True(scrollWidth <= clientWidth + 1, $"Theme layout should not overflow horizontally at {width}px.");

            await page.Keyboard.PressAsync("Tab");
            await page.Keyboard.PressAsync("Tab");
            await page.Keyboard.PressAsync("Tab");
            await Assertions.Expect(page.Locator(":focus")).ToBeVisibleAsync();
        }

        await page.CloseAsync();
    }
}
