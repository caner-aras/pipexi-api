using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

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
