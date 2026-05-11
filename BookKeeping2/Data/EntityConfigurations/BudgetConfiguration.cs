using BookKeeping2.Models.Budgets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeping2.Data.EntityConfigurations;

/// <summary>
/// Configures monthly budget persistence.
/// </summary>
public sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("Budgets");
        builder.HasKey(budget => budget.Id);
        builder.Ignore(budget => budget.Amount);

        builder.HasOne(budget => budget.Category)
            .WithMany()
            .HasForeignKey(budget => budget.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(budget => new { budget.CategoryId, budget.BudgetMonth }).IsUnique();
    }
}
