using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BookKeeping2.Tests.TestSupport;

/// <summary>
/// Creates an in-memory test host for Razor Pages integration tests.
/// </summary>
public sealed class BookKeepingWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}
