using BookKeeping2.Data.SeedData;

namespace BookKeeping2.Data;

/// <summary>
/// Applies database migrations and seed data before the web host accepts requests.
/// </summary>
public sealed class DatabaseStartupService : IHostedService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<DatabaseStartupService> logger;

    /// <summary>
    /// Initializes a new database startup service.
    /// </summary>
    /// <param name="serviceProvider">The application service provider.</param>
    /// <param name="logger">The startup logger.</param>
    public DatabaseStartupService(IServiceProvider serviceProvider, ILogger<DatabaseStartupService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        await DatabaseInitializer.InitializeAsync(scope.ServiceProvider, cancellationToken);
        logger.LogInformation("Bookkeeping database initialized.");
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
