using System.ComponentModel.DataAnnotations;

namespace BookKeeping2.ViewModels.Transactions;

/// <summary>
/// Query string input for transaction filtering.
/// </summary>
public sealed class TransactionFilterInputModel
{
    /// <summary>
    /// Gets or sets the optional inclusive start date.
    /// </summary>
    [Display(Name = "起始日期")]
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the optional inclusive end date.
    /// </summary>
    [Display(Name = "結束日期")]
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the optional category identifier.
    /// </summary>
    [Display(Name = "分類")]
    public long? CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the optional account identifier.
    /// </summary>
    [Display(Name = "帳戶")]
    public long? AccountId { get; set; }

    /// <summary>
    /// Gets or sets the optional minimum amount.
    /// </summary>
    [Range(typeof(decimal), "0", "999999999.99", ErrorMessage = "最小金額不可小於 0。")]
    [Display(Name = "最小金額")]
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// Gets or sets the optional maximum amount.
    /// </summary>
    [Range(typeof(decimal), "0", "999999999.99", ErrorMessage = "最大金額不可小於 0。")]
    [Display(Name = "最大金額")]
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// Gets or sets the optional keyword.
    /// </summary>
    [StringLength(100, ErrorMessage = "關鍵字不可超過 100 個字。")]
    [Display(Name = "關鍵字")]
    public string? Keyword { get; set; }

    /// <summary>
    /// Gets or sets the one-based page number.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; } = 50;
}
