namespace BookKeeping2.Models.Audit;

/// <summary>
/// Represents an auditable event summary.
/// </summary>
public sealed class AuditEvent
{
    /// <summary>
    /// Gets or sets the database identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the event timestamp in UTC.
    /// </summary>
    public DateTimeOffset OccurredAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the event type.
    /// </summary>
    public AuditEventType EventType { get; set; }

    /// <summary>
    /// Gets or sets the affected entity type.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the affected entity identifier summary.
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// Gets or sets the masked event summary.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the severity label.
    /// </summary>
    public string Severity { get; set; } = "Information";

    /// <summary>
    /// Gets or sets the optional correlation identifier.
    /// </summary>
    public string? CorrelationId { get; set; }
}
