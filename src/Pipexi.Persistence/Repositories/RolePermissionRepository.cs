using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class RolePermissionRepository : Repository<RolePermission>, IRolePermissionRepository
{
    public RolePermissionRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<bool> ExistsAsync(
        Guid roleId,
        Guid permissionId,
        Guid? excludingRolePermissionId = null,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(x =>
            x.RoleId == roleId &&
            x.PermissionId == permissionId &&
            (!excludingRolePermissionId.HasValue || x.Id != excludingRolePermissionId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<RolePermission>> ListByRoleIdAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.RoleId == roleId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
