using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookKeeping2.ViewModels.Transactions;

/// <summary>
/// Provides select-list options for transaction forms.
/// </summary>
public sealed class TransactionFormOptionsViewModel
{
    /// <summary>
    /// Gets or sets supported currency options.
    /// </summary>
    public IReadOnlyList<SelectListItem> Currencies { get; set; } = [];

    /// <summary>
    /// Gets or sets category options.
    /// </summary>
    public IReadOnlyList<SelectListItem> Categories { get; set; } = [];

    /// <summary>
    /// Gets or sets account options.
    /// </summary>
    public IReadOnlyList<TransactionAccountOptionViewModel> Accounts { get; set; } = [];
}

/// <summary>
/// Represents an account option in a transaction form.
/// </summary>
public sealed class TransactionAccountOptionViewModel
{
    /// <summary>
    /// Gets or sets the account identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the account name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account currency code.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets the account option display text.
    /// </summary>
    public string DisplayText => $"{Name} ({Currency})";
}
