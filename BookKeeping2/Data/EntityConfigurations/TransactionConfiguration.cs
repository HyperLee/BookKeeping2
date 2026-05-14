using BookKeeping2.Models.Transactions;
using BookKeeping2.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeping2.Data.EntityConfigurations;

/// <summary>
/// Configures transaction persistence.
/// </summary>
public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(transaction => transaction.Id);
        builder.Ignore(transaction => transaction.Amount);

        builder.Property(transaction => transaction.Note).HasMaxLength(500);
        builder.Property(transaction => transaction.DeletionSummary).HasMaxLength(500);
        builder.Property(transaction => transaction.LastChangeSummary).HasMaxLength(500).IsRequired();
        builder.Property(transaction => transaction.Currency)
            .HasMaxLength(3)
            .HasDefaultValue(SupportedCurrency.LegacyDefaultCode)
            .IsRequired();

        builder.HasOne(transaction => transaction.Category)
            .WithMany(category => category.Transactions)
            .HasForeignKey(transaction => transaction.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(transaction => transaction.Account)
            .WithMany(account => account.Transactions)
            .HasForeignKey(transaction => transaction.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(transaction => new { transaction.IsDeleted, transaction.TransactionDate });
        builder.HasIndex(transaction => new { transaction.IsDeleted, transaction.Currency, transaction.TransactionDate });
        builder.HasIndex(transaction => new { transaction.IsDeleted, transaction.CategoryId, transaction.TransactionDate });
        builder.HasIndex(transaction => new { transaction.IsDeleted, transaction.Currency, transaction.CategoryId, transaction.TransactionDate });
        builder.HasIndex(transaction => new { transaction.IsDeleted, transaction.AccountId, transaction.TransactionDate });
        builder.HasIndex(transaction => new { transaction.IsDeleted, transaction.Currency, transaction.AccountId, transaction.TransactionDate });
        builder.HasIndex(transaction => new { transaction.IsDeleted, transaction.AmountMinorUnits });
    }
}
