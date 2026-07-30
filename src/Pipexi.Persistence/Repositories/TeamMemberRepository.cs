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

    public async Task<IReadOnlyCollection<TeamMember>> ListByOrganizationMemberIdsAsync(
        IReadOnlyCollection<Guid> organizationMemberIds,
        CancellationToken cancellationToken = default)
    {
        if (organizationMemberIds.Count == 0)
        {
            return Array.Empty<TeamMember>();
        }

        return await DbSet
            .Where(x => organizationMemberIds.Contains(x.OrganizationMemberId))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> CountByTeamIdsAsync(
        IReadOnlyCollection<Guid> teamIds,
        CancellationToken cancellationToken = default)
    {
        if (teamIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        var counts = await DbSet
            .Where(x => teamIds.Contains(x.TeamId))
            .GroupBy(x => x.TeamId)
            .Select(g => new { TeamId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(x => x.TeamId, x => x.Count);
    }

    public async Task<IReadOnlyCollection<TeamMember>> ListByTeamIdsAndOrganizationMemberIdsAsync(
        IReadOnlyCollection<Guid> teamIds,
        IReadOnlyCollection<Guid> organizationMemberIds,
        CancellationToken cancellationToken = default)
    {
        if (teamIds.Count == 0 || organizationMemberIds.Count == 0)
        {
            return Array.Empty<TeamMember>();
        }

        return await DbSet
            .Where(x =>
                teamIds.Contains(x.TeamId) &&
                organizationMemberIds.Contains(x.OrganizationMemberId))
            .ToListAsync(cancellationToken);
    }
}
