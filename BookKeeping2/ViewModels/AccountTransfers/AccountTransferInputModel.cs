using System.ComponentModel.DataAnnotations;
using BookKeeping2.Models.Common;
using BookKeeping2.Validation;

namespace BookKeeping2.ViewModels.AccountTransfers;

/// <summary>
/// Input model for creating and editing account transfers.
/// </summary>
public sealed class AccountTransferInputModel
{
    /// <summary>
    /// Gets or sets the transfer date.
    /// </summary>
    [Required(ErrorMessage = "請選擇轉帳日期。")]
    [Display(Name = "轉帳日期")]
    public DateOnly TransferDate { get; set; }

    /// <summary>
    /// Gets or sets the supported currency code.
    /// </summary>
    [Required(ErrorMessage = FinancialValidationMessages.CurrencyRequired)]
    [StringLength(3, ErrorMessage = FinancialValidationMessages.CurrencyUnsupported)]
    [Display(Name = "幣別")]
    public string? Currency { get; set; } = SupportedCurrency.LegacyDefaultCode;

    /// <summary>
    /// Gets or sets the transfer amount.
    /// </summary>
    [Required(ErrorMessage = FinancialValidationMessages.AmountRequired)]
    [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = FinancialValidationMessages.AmountTooLarge)]
    [Display(Name = "金額")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the source account identifier.
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "請選擇轉出帳戶。")]
    [Display(Name = "轉出帳戶")]
    public long FromAccountId { get; set; }

    /// <summary>
    /// Gets or sets the destination account identifier.
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "請選擇轉入帳戶。")]
    [Display(Name = "轉入帳戶")]
    public long ToAccountId { get; set; }

    /// <summary>
    /// Gets or sets an optional note.
    /// </summary>
    [StringLength(500, ErrorMessage = "備註不可超過 500 個字。")]
    [Display(Name = "備註")]
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets the create form submission token.
    /// </summary>
    public string? SubmissionToken { get; set; }
}
