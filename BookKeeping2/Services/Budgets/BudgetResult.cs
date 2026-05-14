namespace BookKeeping2.Services.Budgets;

/// <summary>
/// Represents the result of a budget write operation.
/// </summary>
public sealed class BudgetResult
{
    /// <summary>
    /// Error shown when no budget currency is selected.
    /// </summary>
    public const string CurrencyRequiredMessage = "請選擇幣別。";

    /// <summary>
    /// Error shown when the submitted budget currency is not supported.
    /// </summary>
    public const string UnsupportedCurrencyMessage = "幣別不支援，請選擇 TWD、USD、JPY、EUR 或 GBP。";

    /// <summary>
    /// Error shown when a budget already exists for the same month, category, and currency.
    /// </summary>
    public const string DuplicateCategoryMonthCurrencyMessage = "相同月份、分類與幣別的預算已存在。";

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
