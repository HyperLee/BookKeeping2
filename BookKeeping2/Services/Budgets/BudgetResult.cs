namespace BookKeeping2.Services.Budgets;

/// <summary>
/// Represents the result of a budget write operation.
/// </summary>
public sealed class BudgetResult
{
    private readonly Dictionary<string, List<string>> errors = [];

    /// <summary>
    /// Gets whether the operation succeeded.
    /// </summary>
    public bool Succeeded => errors.Count == 0;

    /// <summary>
    /// Gets the affected budget identifier.
    /// </summary>
    public long? BudgetId { get; private set; }

    /// <summary>
    /// Gets validation errors by field.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors => errors.ToDictionary(item => item.Key, item => item.Value.ToArray());

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="budgetId">The affected budget identifier.</param>
    /// <returns>A successful result.</returns>
    public static BudgetResult Success(long? budgetId = null)
    {
        return new BudgetResult { BudgetId = budgetId };
    }

    /// <summary>
    /// Adds a validation error.
    /// </summary>
    /// <param name="field">The related field.</param>
    /// <param name="message">The error message.</param>
    public void AddError(string field, string message)
    {
        if (!errors.TryGetValue(field, out List<string>? messages))
        {
            messages = [];
            errors[field] = messages;
        }

        messages.Add(message);
    }
}
