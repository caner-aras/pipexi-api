using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class TeamMemberRepository : Repository<TeamMember>, ITeamMemberRepository
{
    public TeamMemberRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<bool> ExistsAsync(
        Guid teamId,
        Guid organizationMemberId,
        Guid? excludingTeamMemberId = null,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(x =>
            x.TeamId == teamId &&
            x.OrganizationMemberId == organizationMemberId &&
            (!excludingTeamMemberId.HasValue || x.Id != excludingTeamMemberId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<TeamMember>> ListByTeamIdAsync(
        Guid teamId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.TeamId == teamId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TeamMember>> ListByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.OrganizationMemberId == organizationMemberId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
