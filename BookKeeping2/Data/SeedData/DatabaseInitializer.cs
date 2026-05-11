using BookKeeping2.Services.Time;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping2.Data.SeedData;

/// <summary>
/// Applies database migrations and first-launch seed data.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Migrates and seeds the application database.
    /// </summary>
    /// <param name="serviceProvider">The scoped service provider.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        AppDbContext context = serviceProvider.GetRequiredService<AppDbContext>();
        ITaipeiDateService dateService = serviceProvider.GetRequiredService<ITaipeiDateService>();

        EnsureDatabaseDirectory(context);

        await context.Database.MigrateAsync(cancellationToken);
        await SeedAsync(context, dateService.UtcNow, cancellationToken);
    }

    private static async Task SeedAsync(AppDbContext context, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        if (!await context.Categories.AnyAsync(cancellationToken))
        {
            context.Categories.AddRange(DefaultSeedData.CreateCategories(nowUtc));
        }

        foreach (var setting in DefaultSeedData.CreateSettings(nowUtc))
        {
            bool exists = await context.AppSettings.AnyAsync(existing => existing.Key == setting.Key, cancellationToken);
            if (!exists)
            {
                context.AppSettings.Add(setting);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureDatabaseDirectory(AppDbContext context)
    {
        string? connectionString = context.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var builder = new SqliteConnectionStringBuilder(connectionString);
        string? dataSource = builder.DataSource;
        if (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:")
        {
            return;
        }

        string fullPath = Path.GetFullPath(dataSource);
        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
