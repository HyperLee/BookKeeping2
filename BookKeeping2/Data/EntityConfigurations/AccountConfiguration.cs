using BookKeeping2.Models.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeping2.Data.EntityConfigurations;

/// <summary>
/// Configures account persistence.
/// </summary>
public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(account => account.Id);
        builder.Ignore(account => account.OpeningBalance);

        builder.Property(account => account.Name).HasMaxLength(100).IsRequired();
        builder.Property(account => account.NormalizedName).HasMaxLength(100).IsRequired();
        builder.Property(account => account.IconKey).HasMaxLength(50).IsRequired();
        builder.Property(account => account.Currency).HasMaxLength(3).IsRequired();

        builder.HasIndex(account => account.NormalizedName).IsUnique();
        builder.HasIndex(account => new { account.IsArchived, account.DisplayOrder });
    }
}
