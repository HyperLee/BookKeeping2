namespace BookKeeping2.Services.Transactions;

/// <summary>
/// Represents the outcome of a transaction command.
/// </summary>
public sealed class TransactionResult
{
    private readonly Dictionary<string, List<string>> errors = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets whether the command succeeded.
    /// </summary>
    public bool Succeeded { get; private set; }

    /// <summary>
    /// Gets the affected transaction identifier when available.
    /// </summary>
    public long? TransactionId { get; private set; }

    /// <summary>
    /// Gets validation errors by input property name.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors => errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="transactionId">The affected transaction identifier.</param>
    /// <returns>A successful result.</returns>
    public static TransactionResult Success(long? transactionId = null)
    {
        return new TransactionResult { Succeeded = true, TransactionId = transactionId };
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <returns>A failed result.</returns>
    public static TransactionResult Failure()
    {
        return new TransactionResult { Succeeded = false };
    }

    /// <summary>
    /// Adds a validation error.
    /// </summary>
    /// <param name="field">The input field name.</param>
    /// <param name="message">The Traditional Chinese validation message.</param>
    public void AddError(string field, string message)
    {
        Succeeded = false;

        if (!errors.TryGetValue(field, out List<string>? messages))
        {
            messages = [];
            errors[field] = messages;
        }

        messages.Add(message);
    }
}
