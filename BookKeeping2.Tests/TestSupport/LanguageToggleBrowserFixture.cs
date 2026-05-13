using Microsoft.Playwright;
using Xunit;

namespace BookKeeping2.Tests.TestSupport;

public sealed class LanguageToggleBrowserFixture : IAsyncLifetime
{
    public const string BrowserBaseUrl = "http://bookkeeping-language.test";

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
        HttpRequestMessage request = new(new HttpMethod(route.Request.Method), requestUri.PathAndQuery);
        if (string.Equals(route.Request.Method, HttpMethod.Post.Method, StringComparison.OrdinalIgnoreCase))
        {
            request.Content = new StringContent(route.Request.PostData ?? string.Empty);
            if (route.Request.Headers.TryGetValue("content-type", out string? contentType))
            {
                request.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType);
            }
        }

        if (route.Request.Headers.TryGetValue("cookie", out string? cookieHeader))
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
        }

        HttpResponseMessage response = await client.SendAsync(request);
        string contentTypeValue = response.Content.Headers.ContentType?.ToString() ?? "text/plain";
        string body = IsTextContent(contentTypeValue) ? await response.Content.ReadAsStringAsync() : string.Empty;
        Dictionary<string, string> headers = response.Headers
            .Concat(response.Content.Headers)
            .GroupBy(header => header.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => string.Join("\n", group.SelectMany(header => header.Value)),
                StringComparer.OrdinalIgnoreCase);

        await route.FulfillAsync(new()
        {
            Status = (int)response.StatusCode,
            ContentType = contentTypeValue,
            Body = body,
            Headers = headers
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
