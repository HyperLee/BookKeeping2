using BookKeeping2.Data;
using BookKeeping2.Services.Time;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace BookKeeping2.Tests.TestSupport;

/// <summary>
/// Creates an in-memory test host for Razor Pages integration tests.
/// </summary>
public sealed class BookKeepingWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<ITaipeiDateService>();
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "DataProtectionKeys")));
            services.AddSingleton<ITaipeiDateService>(TestDataBuilder.CreateDateService());
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
        });
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        connection.Dispose();
    }
}
