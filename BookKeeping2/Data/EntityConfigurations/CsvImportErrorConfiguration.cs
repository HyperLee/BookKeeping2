using BookKeeping2.Models.CsvImports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeping2.Data.EntityConfigurations;

/// <summary>
/// Configures CSV import error persistence.
/// </summary>
public sealed class CsvImportErrorConfiguration : IEntityTypeConfiguration<CsvImportError>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CsvImportError> builder)
    {
        builder.ToTable("CsvImportErrors");
        builder.HasKey(error => error.Id);

        builder.Property(error => error.FieldName).HasMaxLength(100);
        builder.Property(error => error.Reason).HasMaxLength(500).IsRequired();
        builder.Property(error => error.RawValuePreview).HasMaxLength(200);

        builder.HasOne(error => error.CsvImportBatch)
            .WithMany(batch => batch.Errors)
            .HasForeignKey(error => error.CsvImportBatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(error => new { error.CsvImportBatchId, error.RowNumber });
    }
}
