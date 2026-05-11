namespace BookKeeping2.Data;

/// <summary>
/// Provides non-secret database path options for the SQLite ledger.
/// </summary>
public sealed class BookKeepingDbOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "BookKeepingDatabase";

    /// <summary>
    /// Gets or sets the directory that stores SQLite database files.
    /// </summary>
    public string DataDirectory { get; set; } = "App_Data";

    /// <summary>
    /// Gets or sets the SQLite database file name.
    /// </summary>
    public string FileName { get; set; } = "bookkeeping.db";

    /// <summary>
    /// Builds a SQLite connection string from the configured path.
    /// </summary>
    /// <returns>A SQLite connection string.</returns>
    public string ToConnectionString()
    {
        string dataSource = Path.Combine(DataDirectory, FileName);
        return $"Data Source={dataSource}";
    }
}
