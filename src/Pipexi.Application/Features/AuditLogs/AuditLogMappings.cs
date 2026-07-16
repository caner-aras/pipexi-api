using Workforce.Application.Features.AuditLogs.Dtos;
using Workforce.Domain.Entities;

namespace Workforce.Application.Features.AuditLogs;

internal static class AuditLogMappings
{
    public static AuditLogDto ToDto(this AuditLog auditLog)
    {
        return new AuditLogDto(
            auditLog.Id,
            auditLog.OrganizationId,
            auditLog.ActorMemberId,
            auditLog.EntityName,
            auditLog.EntityId,
            auditLog.Action,
            auditLog.BeforeJson,
            auditLog.AfterJson,
            auditLog.CreatedAt);
    }
}
