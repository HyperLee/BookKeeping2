using BookKeeping2.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookKeeping2.ViewModels.AccountTransfers;

/// <summary>
/// Provides selectable values for account transfer forms.
/// </summary>
public sealed class AccountTransferFormOptionsViewModel
{
    /// <summary>
    /// Gets currency options.
    /// </summary>
    public IReadOnlyList<SelectListItem> Currencies { get; init; } = SupportedCurrency.Options
        .Select(option => new SelectListItem($"{option.Code} - {option.DisplayName}", option.Code))
        .ToList();

    /// <summary>
    /// Gets active account options.
    /// </summary>
    public IReadOnlyList<AccountTransferAccountOptionViewModel> Accounts { get; init; } = [];
}

/// <summary>
/// Represents one selectable transfer account.
/// </summary>
public sealed class AccountTransferAccountOptionViewModel
{
    /// <summary>
    /// Gets the account identifier.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets the account display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the account currency.
    /// </summary>
    public string Currency { get; init; } = SupportedCurrency.LegacyDefaultCode;

    /// <summary>
    /// Gets the display text shown in transfer account selects.
    /// </summary>
    public string DisplayText => $"{Name} ({Currency})";
}
