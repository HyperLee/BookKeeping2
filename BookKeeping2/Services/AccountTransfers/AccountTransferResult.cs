namespace BookKeeping2.Services.AccountTransfers;

/// <summary>
/// Represents the outcome of an account transfer command.
/// </summary>
public sealed class AccountTransferResult
{
    private readonly Dictionary<string, List<string>> errors = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets whether the command succeeded.
    /// </summary>
    public bool Succeeded { get; private set; }

    /// <summary>
    /// Gets the affected account transfer identifier when available.
    /// </summary>
    public long? TransferId { get; private set; }

    /// <summary>
    /// Gets validation errors by input property name.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors => errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="transferId">The affected transfer identifier.</param>
    /// <returns>A successful result.</returns>
    public static AccountTransferResult Success(long? transferId = null)
    {
        return new AccountTransferResult { Succeeded = true, TransferId = transferId };
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <returns>A failed result.</returns>
    public static AccountTransferResult Failure()
    {
        return new AccountTransferResult { Succeeded = false };
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
