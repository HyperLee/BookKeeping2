using System.ComponentModel.DataAnnotations.Schema;
using BookKeeping2.Models.Categories;
using BookKeeping2.Services.Common;

namespace BookKeeping2.Models.Budgets;

/// <summary>
/// Represents a monthly budget for an expense category.
/// </summary>
public sealed class Budget
{
    /// <summary>
    /// Gets or sets the database identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the expense category identifier.
    /// </summary>
    public long CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the expense category.
    /// </summary>
    public Category Category { get; set; } = null!;

    /// <summary>
    /// Gets or sets the first day of the budget month.
    /// </summary>
    public DateOnly BudgetMonth { get; set; }

    /// <summary>
    /// Gets or sets the budget amount as a decimal TWD amount.
    /// </summary>
    [NotMapped]
    public decimal Amount
    {
        get => MoneyMinorUnitConverter.FromMinorUnits(AmountMinorUnits);
        set => AmountMinorUnits = MoneyMinorUnitConverter.ToMinorUnits(value);
    }

    /// <summary>
    /// Gets or sets the budget amount in minor units.
    /// </summary>
    public long AmountMinorUnits { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp in UTC.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the latest update timestamp in UTC.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
