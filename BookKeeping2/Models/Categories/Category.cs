using BookKeeping2.Models.Common;
using BookKeeping2.Models.Transactions;

namespace BookKeeping2.Models.Categories;

/// <summary>
/// Represents an income or expense category.
/// </summary>
public sealed class Category
{
    /// <summary>
    /// Gets or sets the database identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the normalized unique name.
    /// </summary>
    public string NormalizedName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category transaction type.
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Gets or sets the safe icon key.
    /// </summary>
    public string IconKey { get; set; } = "tag";

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets whether this is a built-in default category.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets whether this category is hidden from new selections.
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp in UTC.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the latest update timestamp in UTC.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets transactions that reference this category.
    /// </summary>
    public ICollection<Transaction> Transactions { get; } = new List<Transaction>();
}
