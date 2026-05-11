using BookKeeping2.Models.CsvImports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeping2.Data.EntityConfigurations;

/// <summary>
/// Configures CSV import batch persistence.
/// </summary>
public sealed class CsvImportBatchConfiguration : IEntityTypeConfiguration<CsvImportBatch>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CsvImportBatch> builder)
    {
        builder.ToTable("CsvImportBatches");
        builder.HasKey(batch => batch.Id);

        builder.Property(batch => batch.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(batch => batch.Summary).HasMaxLength(1000).IsRequired();
    }
}
