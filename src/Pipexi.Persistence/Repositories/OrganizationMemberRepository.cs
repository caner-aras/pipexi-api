using Microsoft.EntityFrameworkCore;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Domain.Entities;
using Workforce.Persistence.Context;

namespace Workforce.Persistence.Repositories;

public sealed class OrganizationMemberRepository : Repository<OrganizationMember>, IOrganizationMemberRepository
{
    public OrganizationMemberRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<OrganizationMember?> GetByOrganizationIdAndUserIdAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.UserId == userId,
            cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid organizationId,
        Guid userId,
        Guid? excludingOrganizationMemberId = null,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(x =>
            x.OrganizationId == organizationId &&
            x.UserId == userId &&
            (!excludingOrganizationMemberId.HasValue || x.Id != excludingOrganizationMemberId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<OrganizationMember>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<OrganizationMember>> ListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
