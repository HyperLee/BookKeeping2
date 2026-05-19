using BookKeeping2.Models.AccountTransfers;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Audit;
using BookKeeping2.Models.Budgets;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.CsvImports;
using BookKeeping2.Models.Settings;
using BookKeeping2.Models.Transactions;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping2.Data;

/// <summary>
/// EF Core database context for Open BookKeeping.
/// </summary>
public sealed class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new context instance.
    /// </summary>
    /// <param name="options">Context options.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the transactions table.
    /// </summary>
    public DbSet<Transaction> Transactions => Set<Transaction>();

    /// <summary>
    /// Gets the account transfers table.
    /// </summary>
    public DbSet<AccountTransfer> AccountTransfers => Set<AccountTransfer>();

    /// <summary>
    /// Gets the categories table.
    /// </summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>
    /// Gets the accounts table.
    /// </summary>
    public DbSet<Account> Accounts => Set<Account>();

    /// <summary>
    /// Gets the budgets table.
    /// </summary>
    public DbSet<Budget> Budgets => Set<Budget>();

    /// <summary>
    /// Gets the CSV import batches table.
    /// </summary>
    public DbSet<CsvImportBatch> CsvImportBatches => Set<CsvImportBatch>();

    /// <summary>
    /// Gets the CSV import errors table.
    /// </summary>
    public DbSet<CsvImportError> CsvImportErrors => Set<CsvImportError>();

    /// <summary>
    /// Gets the audit events table.
    /// </summary>
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    /// <summary>
    /// Gets the application settings table.
    /// </summary>
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
