namespace BookKeeping2.Services.Accounts;

/// <summary>
/// Represents the result of an account command.
/// </summary>
public sealed class AccountResult
{
    private readonly Dictionary<string, List<string>> errors = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets whether the command succeeded.
    /// </summary>
    public bool Succeeded { get; private set; } = true;

    /// <summary>
    /// Gets validation errors by field name.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors => errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());

    /// <summary>
    /// Adds a validation error.
    /// </summary>
    /// <param name="field">The field name.</param>
    /// <param name="message">The error message.</param>
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
