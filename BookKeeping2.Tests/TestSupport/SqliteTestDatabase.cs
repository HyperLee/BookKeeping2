using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping2.Tests.TestSupport;

/// <summary>
/// Provides an open in-memory SQLite connection for tests that need relational behavior.
/// </summary>
public sealed class SqliteTestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection connection;

    /// <summary>
    /// Initializes a new SQLite in-memory database and keeps the connection open.
    /// </summary>
    public SqliteTestDatabase()
    {
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
    }

    /// <summary>
    /// Creates DbContext options bound to the shared in-memory connection.
    /// </summary>
    /// <typeparam name="TContext">The EF Core DbContext type under test.</typeparam>
    /// <returns>Options that use the test SQLite connection.</returns>
    public DbContextOptions<TContext> CreateOptions<TContext>()
        where TContext : DbContext
    {
        return new DbContextOptionsBuilder<TContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await connection.DisposeAsync();
    }
}
