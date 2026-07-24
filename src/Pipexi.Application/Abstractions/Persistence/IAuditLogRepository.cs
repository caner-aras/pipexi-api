using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IReadOnlyCollection<AuditLog>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
