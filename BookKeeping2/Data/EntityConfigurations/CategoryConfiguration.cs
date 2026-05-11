using BookKeeping2.Models.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeping2.Data.EntityConfigurations;

/// <summary>
/// Configures category persistence.
/// </summary>
public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name).HasMaxLength(100).IsRequired();
        builder.Property(category => category.NormalizedName).HasMaxLength(100).IsRequired();
        builder.Property(category => category.IconKey).HasMaxLength(50).IsRequired();

        builder.HasIndex(category => new { category.Type, category.NormalizedName }).IsUnique();
        builder.HasIndex(category => new { category.Type, category.IsArchived, category.DisplayOrder });
    }
}
