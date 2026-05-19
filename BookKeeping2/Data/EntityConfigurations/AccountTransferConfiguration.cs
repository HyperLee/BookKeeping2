using BookKeeping2.Models.AccountTransfers;
using BookKeeping2.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeping2.Data.EntityConfigurations;

/// <summary>
/// Configures account transfer persistence.
/// </summary>
public sealed class AccountTransferConfiguration : IEntityTypeConfiguration<AccountTransfer>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AccountTransfer> builder)
    {
        builder.ToTable("AccountTransfers");
        builder.HasKey(transfer => transfer.Id);
        builder.Ignore(transfer => transfer.Amount);

        builder.Property(transfer => transfer.Currency)
            .HasMaxLength(3)
            .HasDefaultValue(SupportedCurrency.LegacyDefaultCode)
            .IsRequired();
        builder.Property(transfer => transfer.Note).HasMaxLength(500);
        builder.Property(transfer => transfer.SubmissionToken).HasMaxLength(64).IsRequired();
        builder.Property(transfer => transfer.DeletionSummary).HasMaxLength(500);
        builder.Property(transfer => transfer.LastChangeSummary).HasMaxLength(500).IsRequired();

        builder.HasOne(transfer => transfer.FromAccount)
            .WithMany()
            .HasForeignKey(transfer => transfer.FromAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(transfer => transfer.ToAccount)
            .WithMany()
            .HasForeignKey(transfer => transfer.ToAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(transfer => transfer.SubmissionToken).IsUnique();
        builder.HasIndex(transfer => new { transfer.IsDeleted, transfer.TransferDate });
        builder.HasIndex(transfer => new { transfer.IsDeleted, transfer.Currency, transfer.TransferDate });
        builder.HasIndex(transfer => new { transfer.IsDeleted, transfer.FromAccountId, transfer.TransferDate });
        builder.HasIndex(transfer => new { transfer.IsDeleted, transfer.ToAccountId, transfer.TransferDate });
    }
}
