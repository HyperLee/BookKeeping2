using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookKeeping2.Data;

/// <summary>
/// Creates the application DbContext for design-time EF Core tools.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <inheritdoc />
    public AppDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseSqlite("Data Source=App_Data/bookkeeping.db");
        return new AppDbContext(builder.Options);
    }
}
