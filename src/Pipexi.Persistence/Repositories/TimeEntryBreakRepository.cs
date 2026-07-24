using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class TimeEntryBreakRepository : Repository<TimeEntryBreak>, ITimeEntryBreakRepository
{
    public TimeEntryBreakRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyCollection<TimeEntryBreak>> ListByTimeEntryIdAsync(
        Guid timeEntryId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.TimeEntryId == timeEntryId)
            .OrderBy(x => x.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TimeEntryBreak>> ListByTimeEntryIdsAsync(
        IReadOnlyCollection<Guid> timeEntryIds,
        CancellationToken cancellationToken = default)
    {
        if (timeEntryIds.Count == 0)
        {
            return Array.Empty<TimeEntryBreak>();
        }

        return await DbSet
            .Where(x => timeEntryIds.Contains(x.TimeEntryId))
            .OrderBy(x => x.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> OverlapsAsync(
        Guid timeEntryId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        Guid? excludingTimeEntryBreakId = null,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(x =>
            x.TimeEntryId == timeEntryId &&
            (!excludingTimeEntryBreakId.HasValue || x.Id != excludingTimeEntryBreakId.Value) &&
            startAt < x.EndAt &&
            endAt > x.StartAt,
            cancellationToken);
    }
}
