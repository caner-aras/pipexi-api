using Microsoft.EntityFrameworkCore;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Domain.Entities;
using Workforce.Persistence.Context;

namespace Workforce.Persistence.Repositories;

public sealed class LocationRepository : Repository<Location>, ILocationRepository
{
    public LocationRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<bool> NameExistsAsync(
        Guid organizationId,
        string name,
        Guid? excludingLocationId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLowerInvariant();

        return await DbSet.AnyAsync(x =>
            x.OrganizationId == organizationId &&
            x.Name.ToLower() == normalizedName &&
            (!excludingLocationId.HasValue || x.Id != excludingLocationId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<Location>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
