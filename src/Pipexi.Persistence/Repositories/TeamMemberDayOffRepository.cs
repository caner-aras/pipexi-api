using Microsoft.EntityFrameworkCore;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Domain.Entities;
using Workforce.Persistence.Context;

namespace Workforce.Persistence.Repositories;

public sealed class TeamMemberDayOffRepository : Repository<TeamMemberDayOff>, ITeamMemberDayOffRepository
{
    public TeamMemberDayOffRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyCollection<TeamMemberDayOff>> ListByTeamMemberIdAsync(
        Guid teamMemberId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.TeamMemberId == teamMemberId)
            .OrderBy(x => x.StartAt)
            .ThenBy(x => x.EndAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TeamMemberDayOff>> ListByTeamMemberIdsAsync(
        IReadOnlyCollection<Guid> teamMemberIds,
        CancellationToken cancellationToken = default)
    {
        if (teamMemberIds.Count == 0)
        {
            return [];
        }

        return await DbSet
            .Where(x => teamMemberIds.Contains(x.TeamMemberId))
            .OrderBy(x => x.StartAt)
            .ThenBy(x => x.EndAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasOverlapAsync(
        Guid teamMemberId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        Guid? excludingDayOffId = null,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            x => x.TeamMemberId == teamMemberId &&
                 (!excludingDayOffId.HasValue || x.Id != excludingDayOffId.Value) &&
                 x.StartAt < endAt &&
                 startAt < x.EndAt,
            cancellationToken);
    }

    public async Task<bool> HasOverlapForTeamMembersAsync(
        IReadOnlyCollection<Guid> teamMemberIds,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        CancellationToken cancellationToken = default)
    {
        if (teamMemberIds.Count == 0)
        {
            return false;
        }

        return await DbSet.AnyAsync(
            x => teamMemberIds.Contains(x.TeamMemberId) &&
                 x.StartAt < endAt &&
                 startAt < x.EndAt,
            cancellationToken);
    }
}