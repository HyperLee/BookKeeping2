using BookKeeping2.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeping2.Data.EntityConfigurations;

/// <summary>
/// Configures non-secret application setting persistence.
/// </summary>
public sealed class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable("AppSettings");
        builder.HasKey(setting => setting.Key);

        builder.Property(setting => setting.Key).HasMaxLength(100).IsRequired();
        builder.Property(setting => setting.Value).HasMaxLength(500).IsRequired();
    }
}
