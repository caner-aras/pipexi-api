namespace Pipexi.Application.Features.AuditLogs.Dtos;

public sealed record AuditLogDto(
    Guid Id,
    Guid OrganizationId,
    Guid? ActorMemberId,
    string EntityName,
    Guid EntityId,
    string Action,
    string? BeforeJson,
    string? AfterJson,
    DateTimeOffset CreatedAt);
