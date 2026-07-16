using Microsoft.EntityFrameworkCore;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Domain.Entities;
using Workforce.Persistence.Context;

namespace Workforce.Persistence.Repositories;

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
