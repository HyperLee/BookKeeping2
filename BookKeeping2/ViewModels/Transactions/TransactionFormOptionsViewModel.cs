using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookKeeping2.ViewModels.Transactions;

/// <summary>
/// Provides select-list options for transaction forms.
/// </summary>
public sealed class TransactionFormOptionsViewModel
{
    /// <summary>
    /// Gets or sets category options.
    /// </summary>
    public IReadOnlyList<SelectListItem> Categories { get; set; } = [];

    /// <summary>
    /// Gets or sets account options.
    /// </summary>
    public IReadOnlyList<SelectListItem> Accounts { get; set; } = [];
}
