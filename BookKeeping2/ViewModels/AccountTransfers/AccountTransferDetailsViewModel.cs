namespace BookKeeping2.ViewModels.AccountTransfers;

/// <summary>
/// Read model for displaying account transfer details.
/// </summary>
public sealed class AccountTransferDetailsViewModel
{
    /// <summary>
    /// Gets the transfer identifier.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets the transfer date.
    /// </summary>
    public DateOnly TransferDate { get; init; }

    /// <summary>
    /// Gets the currency code.
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Gets the transfer amount.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Gets the source account name.
    /// </summary>
    public string FromAccountName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the destination account name.
    /// </summary>
    public string ToAccountName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional note.
    /// </summary>
    public string? Note { get; init; }

    /// <summary>
    /// Gets the formatted amount text.
    /// </summary>
    public string AmountText => $"{Currency} {Amount:N2}";

    /// <summary>
    /// Gets the formatted transfer direction.
    /// </summary>
    public string DirectionText => $"{FromAccountName} -> {ToAccountName}";
}
