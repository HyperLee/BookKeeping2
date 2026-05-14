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
        using var request = new HttpRequestMessage(new HttpMethod(route.Request.Method), pathAndQuery);
        CopyRequestHeaders(route.Request, request);
        byte[]? postData = route.Request.PostDataBuffer;
        if (postData is { Length: > 0 })
        {
            request.Content = new ByteArrayContent(postData);
            CopyContentHeaders(route.Request, request.Content);
        }

        HttpResponseMessage response = await client.SendAsync(request);
        string contentType = response.Content.Headers.ContentType?.ToString() ?? "text/plain";
        byte[] body = await response.Content.ReadAsByteArrayAsync();
        await route.FulfillAsync(new()
        {
            Status = (int)response.StatusCode,
            ContentType = contentType,
            Headers = BuildResponseHeaders(response),
            BodyBytes = body
        });
    }

    private static void CopyRequestHeaders(IRequest source, HttpRequestMessage target)
    {
        foreach ((string key, string value) in source.Headers)
        {
            if (key.Equals("host", StringComparison.OrdinalIgnoreCase)
                || key.Equals("content-length", StringComparison.OrdinalIgnoreCase)
                || key.Equals("content-type", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            target.Headers.TryAddWithoutValidation(key, value);
        }
    }

    private static void CopyContentHeaders(IRequest source, HttpContent target)
    {
        if (source.Headers.TryGetValue("content-type", out string? contentType))
        {
            target.Headers.TryAddWithoutValidation("Content-Type", contentType);
        }
    }

    private static IReadOnlyList<KeyValuePair<string, string>> BuildResponseHeaders(HttpResponseMessage response)
    {
        var headers = new List<KeyValuePair<string, string>>();
        foreach ((string? key, IEnumerable<string> values) in response.Headers)
        {
            foreach (string value in values)
            {
                headers.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        foreach ((string? key, IEnumerable<string> values) in response.Content.Headers)
        {
            foreach (string value in values)
            {
                headers.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        return headers;
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
