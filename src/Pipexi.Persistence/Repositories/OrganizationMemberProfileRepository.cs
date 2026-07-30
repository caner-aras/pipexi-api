using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class OrganizationMemberProfileRepository(ApplicationDbContext dbContext)
    : Repository<OrganizationMemberProfile>(dbContext), IOrganizationMemberProfileRepository
{
    public async Task<OrganizationMemberProfile?> GetByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(x => x.OrganizationMemberId == organizationMemberId, cancellationToken);
    }
}
