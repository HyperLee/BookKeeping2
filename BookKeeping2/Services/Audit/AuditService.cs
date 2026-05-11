using BookKeeping2.Data;
using BookKeeping2.Models.Audit;
using BookKeeping2.Services.Time;

namespace BookKeeping2.Services.Audit;

/// <summary>
/// Persists masked audit events and writes matching structured logs.
/// </summary>
public sealed class AuditService : IAuditService
{
    private readonly AppDbContext dbContext;
    private readonly ITaipeiDateService dateService;
    private readonly ILogger<AuditService> logger;

    /// <summary>
    /// Initializes a new audit service.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    /// <param name="dateService">The time service.</param>
    /// <param name="logger">The structured logger.</param>
    public AuditService(AppDbContext dbContext, ITaipeiDateService dateService, ILogger<AuditService> logger)
    {
        this.dbContext = dbContext;
        this.dateService = dateService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task RecordAsync(
        AuditEventType eventType,
        string entityType,
        string? entityId,
        string summary,
        string severity = "Information",
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var auditEvent = new AuditEvent
        {
            EventType = eventType,
            EntityType = entityType,
            EntityId = entityId,
            Summary = summary,
            Severity = severity,
            CorrelationId = correlationId,
            OccurredAtUtc = dateService.UtcNow
        };

        dbContext.AuditEvents.Add(auditEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Audit event {EventType} for {EntityType}/{EntityId}: {Summary}",
            eventType,
            entityType,
            entityId,
            summary);
    }
}
