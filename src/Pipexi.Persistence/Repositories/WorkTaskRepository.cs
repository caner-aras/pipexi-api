using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class WorkTaskRepository : Repository<WorkTask>, IWorkTaskRepository
{
    public WorkTaskRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyCollection<WorkTask>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.DueAt)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<WorkTask>> ListByAssignedTeamMemberIdAsync(
        Guid teamMemberId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.AssignedToTeamMemberId == teamMemberId)
            .OrderBy(x => x.DueAt)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<WorkTask>> ListByAssignedTeamMemberIdsAsync(
        IReadOnlyCollection<Guid> teamMemberIds,
        CancellationToken cancellationToken = default)
    {
        if (teamMemberIds.Count == 0)
        {
            return [];
        }

        return await DbSet
            .Where(x => x.AssignedToTeamMemberId.HasValue && teamMemberIds.Contains(x.AssignedToTeamMemberId.Value))
            .OrderBy(x => x.DueAt)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<WorkTask>> ListByAssignedTeamIdAsync(
        Guid teamId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.AssignedToTeamId == teamId)
            .OrderBy(x => x.DueAt)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
