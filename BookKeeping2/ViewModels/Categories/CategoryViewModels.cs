using System.ComponentModel.DataAnnotations;
using BookKeeping2.Models.Common;

namespace BookKeeping2.ViewModels.Categories;

/// <summary>
/// Input model for category management.
/// </summary>
public sealed class CategoryInputModel
{
    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    [Required(ErrorMessage = "請輸入分類名稱。")]
    [StringLength(100, ErrorMessage = "分類名稱不可超過 100 個字。")]
    [Display(Name = "名稱")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category type.
    /// </summary>
    [Display(Name = "類型")]
    public TransactionType Type { get; set; } = TransactionType.Expense;

    /// <summary>
    /// Gets or sets the icon key.
    /// </summary>
    [StringLength(50)]
    [Display(Name = "圖示")]
    public string IconKey { get; set; } = "tag";

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    [Display(Name = "排序")]
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Category row shown on management pages.
/// </summary>
public sealed class CategoryListItemViewModel
{
    /// <summary>
    /// Gets or sets the category identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category type.
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Gets the Traditional Chinese type label.
    /// </summary>
    public string TypeText => Type == TransactionType.Income ? "收入" : "支出";

    /// <summary>
    /// Gets or sets whether the category is default.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets whether the category is archived.
    /// </summary>
    public bool IsArchived { get; set; }
}
