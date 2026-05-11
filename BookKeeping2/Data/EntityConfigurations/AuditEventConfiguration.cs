using BookKeeping2.Models.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeping2.Data.EntityConfigurations;

/// <summary>
/// Configures audit event persistence.
/// </summary>
public sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("AuditEvents");
        builder.HasKey(auditEvent => auditEvent.Id);

        builder.Property(auditEvent => auditEvent.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(auditEvent => auditEvent.EntityId).HasMaxLength(100);
        builder.Property(auditEvent => auditEvent.Summary).HasMaxLength(1000).IsRequired();
        builder.Property(auditEvent => auditEvent.Severity).HasMaxLength(20).IsRequired();
        builder.Property(auditEvent => auditEvent.CorrelationId).HasMaxLength(100);

        builder.HasIndex(auditEvent => new { auditEvent.EventType, auditEvent.OccurredAtUtc });
        builder.HasIndex(auditEvent => new { auditEvent.EntityType, auditEvent.EntityId });
    }
}
