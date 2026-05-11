namespace BookKeeping2.Models.Common;

/// <summary>
/// Indicates whether a transaction increases or decreases available funds.
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Income transaction.
    /// </summary>
    Income = 1,

    /// <summary>
    /// Expense transaction.
    /// </summary>
    Expense = 2
}
