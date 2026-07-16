using Microsoft.EntityFrameworkCore;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Domain.Entities;
using Workforce.Persistence.Context;

namespace Workforce.Persistence.Repositories;

public sealed class TimeEntryRepository : Repository<TimeEntry>, ITimeEntryRepository
{
    public TimeEntryRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyCollection<TimeEntry>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.ClockInAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TimeEntry>> ListByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.OrganizationMemberId == organizationMemberId)
            .OrderBy(x => x.ClockInAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TimeEntry>> ListByShiftIdsAsync(
        IReadOnlyCollection<Guid> shiftIds,
        CancellationToken cancellationToken = default)
    {
        if (shiftIds.Count == 0)
        {
            return Array.Empty<TimeEntry>();
        }

        return await DbSet
            .Where(x => shiftIds.Contains(x.ShiftId))
            .OrderBy(x => x.ClockInAt)
            .ToListAsync(cancellationToken);
    }
}
