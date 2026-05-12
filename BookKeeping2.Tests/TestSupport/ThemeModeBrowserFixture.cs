using Microsoft.Playwright;
using Xunit;

namespace BookKeeping2.Tests.TestSupport;

public sealed class ThemeModeBrowserFixture : IAsyncLifetime
{
    public const string BrowserBaseUrl = "http://bookkeeping-theme.test";

    private IPlaywright? playwright;
    private IBrowser? browser;

    public async Task InitializeAsync()
    {
        playwright = await Playwright.CreateAsync();
        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = true,
            ExecutablePath = ResolveBrowserExecutable()
        };
        browser = await playwright.Chromium.LaunchAsync(launchOptions);
    }

    public async Task DisposeAsync()
    {
        if (browser is not null)
        {
            await browser.DisposeAsync();
        }

        playwright?.Dispose();
    }

    public async Task<IPage> NewPageAsync(BookKeepingWebApplicationFactory factory)
    {
        IBrowserContext context = await NewContextAsync(factory);
        return await context.NewPageAsync();
    }

    public async Task<IBrowserContext> NewContextAsync(BookKeepingWebApplicationFactory factory)
    {
        if (browser is null)
        {
            throw new InvalidOperationException("Browser fixture has not been initialized.");
        }

        IBrowserContext context = await browser.NewContextAsync(new() { BaseURL = BrowserBaseUrl });
        HttpClient client = factory.CreateClient();
        await context.RouteAsync($"{BrowserBaseUrl}/**", route => FulfillFromFactoryAsync(route, client));
        return context;
    }

    private static async Task FulfillFromFactoryAsync(IRoute route, HttpClient client)
    {
        Uri requestUri = new(route.Request.Url);
        string pathAndQuery = requestUri.PathAndQuery;
        HttpResponseMessage response = await client.GetAsync(pathAndQuery);
        string contentType = response.Content.Headers.ContentType?.ToString() ?? "text/plain";
        string body = IsTextContent(contentType) ? await response.Content.ReadAsStringAsync() : string.Empty;
        await route.FulfillAsync(new()
        {
            Status = (int)response.StatusCode,
            ContentType = contentType,
            Body = body
        });
    }

    private static bool IsTextContent(string contentType)
    {
        return contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("javascript", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("json", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ResolveBrowserExecutable()
    {
        string[] candidates =
        [
            @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
            @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
        ];

        return candidates.FirstOrDefault(File.Exists);
    }
}
