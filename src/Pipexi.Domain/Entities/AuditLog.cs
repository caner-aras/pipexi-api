namespace Workforce.Domain.Entities;

public sealed class AuditLog : BaseEntity
{
    private AuditLog(
        Guid id,
        Guid organizationId,
        Guid? actorMemberId,
        string entityName,
        Guid entityId,
        string action,
        string? beforeJson,
        string? afterJson,
        DateTimeOffset createdAt)
        : base(id, "active", createdAt)
    {
        OrganizationId = organizationId;
        ActorMemberId = actorMemberId;
        EntityName = entityName;
        EntityId = entityId;
        Action = action;
        BeforeJson = beforeJson;
        AfterJson = afterJson;
    }

    public Guid OrganizationId { get; private set; }
    public Guid? ActorMemberId { get; private set; }
    public string EntityName { get; private set; }
    public Guid EntityId { get; private set; }
    public string Action { get; private set; }
    public string? BeforeJson { get; private set; }
    public string? AfterJson { get; private set; }

    public static AuditLog Create(
        Guid organizationId,
        Guid? actorMemberId,
        string entityName,
        Guid entityId,
        string action,
        string? beforeJson,
        string? afterJson,
        DateTimeOffset? createdAt)
    {
        return new AuditLog(
            Guid.NewGuid(),
            organizationId,
            actorMemberId,
            entityName.Trim(),
            entityId,
            action.Trim().ToLowerInvariant(),
            string.IsNullOrWhiteSpace(beforeJson) ? null : beforeJson,
            string.IsNullOrWhiteSpace(afterJson) ? null : afterJson,
            createdAt ?? DateTimeOffset.UtcNow);
    }
}
