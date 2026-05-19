namespace BookKeeping2.Models.Audit;

/// <summary>
/// Represents auditable business events in the bookkeeping system.
/// </summary>
public enum AuditEventType
{
    /// <summary>
    /// A transaction was created.
    /// </summary>
    TransactionCreated = 1,

    /// <summary>
    /// A transaction was updated.
    /// </summary>
    TransactionUpdated = 2,

    /// <summary>
    /// A transaction was soft deleted.
    /// </summary>
    TransactionDeleted = 3,

    /// <summary>
    /// A CSV file was imported.
    /// </summary>
    CsvImported = 4,

    /// <summary>
    /// Transactions were exported to CSV.
    /// </summary>
    CsvExported = 5,

    /// <summary>
    /// A budget warning was triggered.
    /// </summary>
    BudgetWarningTriggered = 6,

    /// <summary>
    /// An account transfer was created.
    /// </summary>
    TransferCreated = 7,

    /// <summary>
    /// An account transfer was updated.
    /// </summary>
    TransferUpdated = 8,

    /// <summary>
    /// An account transfer was soft deleted.
    /// </summary>
    TransferDeleted = 9,

    /// <summary>
    /// Account transfers were imported from CSV.
    /// </summary>
    TransferCsvImported = 10,

    /// <summary>
    /// Account transfers were exported to CSV.
    /// </summary>
    TransferCsvExported = 11,

    /// <summary>
    /// An unexpected error occurred.
    /// </summary>
    UnexpectedError = 99
}
