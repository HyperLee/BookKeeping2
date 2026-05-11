using BookKeeping2.Models.Audit;

namespace BookKeeping2.Services.Audit;

/// <summary>
/// Records masked audit events.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Records a masked audit event.
    /// </summary>
    /// <param name="eventType">The event type.</param>
    /// <param name="entityType">The affected entity type.</param>
    /// <param name="entityId">The affected entity identifier summary.</param>
    /// <param name="summary">The masked event summary.</param>
    /// <param name="severity">The severity label.</param>
    /// <param name="correlationId">The optional correlation identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordAsync(
        AuditEventType eventType,
        string entityType,
        string? entityId,
        string summary,
        string severity = "Information",
        string? correlationId = null,
        CancellationToken cancellationToken = default);
}
