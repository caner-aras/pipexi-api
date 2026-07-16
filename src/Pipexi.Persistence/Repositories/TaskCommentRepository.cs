using Microsoft.EntityFrameworkCore;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Domain.Entities;
using Workforce.Persistence.Context;

namespace Workforce.Persistence.Repositories;

public sealed class TaskCommentRepository : Repository<TaskComment>, ITaskCommentRepository
{
    public TaskCommentRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyCollection<TaskComment>> ListByWorkTaskIdAsync(
        Guid workTaskId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.WorkTaskId == workTaskId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TaskComment>> ListByWorkTaskIdsAsync(
        IReadOnlyCollection<Guid> workTaskIds,
        CancellationToken cancellationToken = default)
    {
        if (workTaskIds.Count == 0)
        {
            return Array.Empty<TaskComment>();
        }

        return await DbSet
            .Where(x => workTaskIds.Contains(x.WorkTaskId))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TaskComment>> ListByTeamMemberIdsAsync(
        IReadOnlyCollection<Guid> teamMemberIds,
        CancellationToken cancellationToken = default)
    {
        if (teamMemberIds.Count == 0)
        {
            return Array.Empty<TaskComment>();
        }

        return await DbSet
            .Where(x => teamMemberIds.Contains(x.TeamMemberId))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
