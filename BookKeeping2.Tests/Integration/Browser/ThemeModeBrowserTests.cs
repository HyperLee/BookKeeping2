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
}
