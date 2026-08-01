using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

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

    public async Task<IReadOnlyCollection<TeamMemberDayOff>> ListPendingByTeamMemberIdsAsync(
        IReadOnlyCollection<Guid> teamMemberIds,
        CancellationToken cancellationToken = default)
    {
        if (teamMemberIds.Count == 0)
        {
            return [];
        }

        return await DbSet
            .Where(x => teamMemberIds.Contains(x.TeamMemberId) && x.Status == "pending")
            .OrderBy(x => x.StartAt)
            .ThenBy(x => x.EndAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TeamMemberDayOff>> ListActiveByTeamMemberIdsAsync(
        IReadOnlyCollection<Guid> teamMemberIds,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        if (teamMemberIds.Count == 0)
        {
            return [];
        }

        return await DbSet
            .Where(x => teamMemberIds.Contains(x.TeamMemberId)
                        && x.Status == "active"
                        && x.StartAt <= now
                        && x.EndAt > now)
            .OrderBy(x => x.StartAt)
            .ThenBy(x => x.EndAt)
            .ToListAsync(cancellationToken);
    }
}