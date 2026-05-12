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
}
