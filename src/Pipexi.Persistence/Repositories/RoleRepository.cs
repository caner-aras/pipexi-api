using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<bool> NameExistsAsync(
        Guid organizationId,
        string name,
        Guid? excludingRoleId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLowerInvariant();

        return await DbSet.AnyAsync(x =>
            x.OrganizationId == organizationId &&
            x.Name.ToLower() == normalizedName &&
            (!excludingRoleId.HasValue || x.Id != excludingRoleId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<Role>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
