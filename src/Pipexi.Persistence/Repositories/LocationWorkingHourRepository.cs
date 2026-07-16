using Microsoft.EntityFrameworkCore;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Domain.Entities;
using Workforce.Persistence.Context;

namespace Workforce.Persistence.Repositories;

public sealed class LocationWorkingHourRepository : Repository<LocationWorkingHour>, ILocationWorkingHourRepository
{
    public LocationWorkingHourRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyCollection<LocationWorkingHour>> ListByLocationIdAsync(
        Guid locationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.LocationId == locationId)
            .OrderBy(x => x.DayOfWeek)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<LocationWorkingHour>> ListByLocationIdsAsync(
        IReadOnlyCollection<Guid> locationIds,
        CancellationToken cancellationToken = default)
    {
        if (locationIds.Count == 0)
        {
            return [];
        }

        var ids = locationIds.Distinct().ToArray();

        return await DbSet
            .Where(x => ids.Contains(x.LocationId))
            .OrderBy(x => x.LocationId)
            .ThenBy(x => x.DayOfWeek)
            .ToListAsync(cancellationToken);
    }

    public async Task HardDeleteByLocationIdAsync(
        Guid locationId,
        CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .IgnoreQueryFilters()
            .Where(x => x.LocationId == locationId)
            .ToListAsync(cancellationToken);

        if (existing.Count == 0)
        {
            return;
        }

        DbSet.RemoveRange(existing);
    }
}