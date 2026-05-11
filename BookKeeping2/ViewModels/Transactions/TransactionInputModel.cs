using System.ComponentModel.DataAnnotations;
using BookKeeping2.Models.Common;
using BookKeeping2.Validation;

namespace BookKeeping2.ViewModels.Transactions;

/// <summary>
/// Input model for creating and editing transactions.
/// </summary>
public sealed class TransactionInputModel
{
    /// <summary>
    /// Gets or sets the transaction date.
    /// </summary>
    [Required(ErrorMessage = "請選擇交易日期。")]
    [Display(Name = "日期")]
    public DateOnly TransactionDate { get; set; }

    /// <summary>
    /// Gets or sets the transaction type.
    /// </summary>
    [Required(ErrorMessage = "請選擇類型。")]
    [Display(Name = "類型")]
    public TransactionType Type { get; set; } = TransactionType.Expense;

    /// <summary>
    /// Gets or sets the TWD amount.
    /// </summary>
    [Required(ErrorMessage = FinancialValidationMessages.AmountRequired)]
    [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = FinancialValidationMessages.AmountTooLarge)]
    [Display(Name = "金額")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the category identifier.
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = FinancialValidationMessages.CategoryRequired)]
    [Display(Name = "分類")]
    public long CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the account identifier.
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = FinancialValidationMessages.AccountRequired)]
    [Display(Name = "帳戶")]
    public long AccountId { get; set; }

    /// <summary>
    /// Gets or sets an optional note.
    /// </summary>
    [StringLength(500, ErrorMessage = "備註不可超過 500 個字。")]
    [Display(Name = "備註")]
    public string? Note { get; set; }
}
