namespace Pipexi.Contracts.V1.AuditLogs;

public sealed record CreateAuditLogRequest(
    Guid OrganizationId,
    Guid? ActorMemberId,
    string EntityName,
    Guid EntityId,
    string Action,
    string? BeforeJson,
    string? AfterJson,
    DateTimeOffset? CreatedAt = null);
