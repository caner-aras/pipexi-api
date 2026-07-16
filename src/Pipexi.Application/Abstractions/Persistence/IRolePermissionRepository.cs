using Workforce.Domain.Entities;

namespace Workforce.Application.Abstractions.Persistence;

public interface IRolePermissionRepository : IRepository<RolePermission>
{
    Task<bool> ExistsAsync(Guid roleId, Guid permissionId, Guid? excludingRolePermissionId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RolePermission>> ListByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
}
