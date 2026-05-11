using System.ComponentModel.DataAnnotations;
using BookKeeping2.Models.Accounts;

namespace BookKeeping2.ViewModels.Accounts;

/// <summary>
/// Input model for account management.
/// </summary>
public sealed class AccountInputModel
{
    /// <summary>
    /// Gets or sets the account name.
    /// </summary>
    [Required(ErrorMessage = "請輸入帳戶名稱。")]
    [StringLength(100, ErrorMessage = "帳戶名稱不可超過 100 個字。")]
    [Display(Name = "名稱")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account type.
    /// </summary>
    [Display(Name = "類型")]
    public AccountType Type { get; set; } = AccountType.Cash;

    /// <summary>
    /// Gets or sets the opening balance.
    /// </summary>
    [Display(Name = "初始餘額")]
    public decimal OpeningBalance { get; set; }

    /// <summary>
    /// Gets or sets the icon key.
    /// </summary>
    [StringLength(50)]
    [Display(Name = "圖示")]
    public string IconKey { get; set; } = "wallet";

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    [Display(Name = "排序")]
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Account row shown on management pages.
/// </summary>
public sealed class AccountListItemViewModel
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
    /// Gets or sets the account type.
    /// </summary>
    public AccountType Type { get; set; }

    /// <summary>
    /// Gets the Traditional Chinese account type label.
    /// </summary>
    public string TypeText => Type switch
    {
        AccountType.Bank => "銀行",
        AccountType.CreditCard => "信用卡",
        AccountType.EWallet => "電子支付",
        AccountType.Other => "其他",
        _ => "現金"
    };

    /// <summary>
    /// Gets or sets the current balance.
    /// </summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>
    /// Gets or sets whether the account is archived.
    /// </summary>
    public bool IsArchived { get; set; }
}
