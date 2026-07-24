using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class OrganizationRepository : Repository<Organization>, IOrganizationRepository
{
    public OrganizationRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludingOrganizationId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();

        return await DbSet.AnyAsync(x =>
            x.Slug.ToLower() == normalizedSlug &&
            (!excludingOrganizationId.HasValue || x.Id != excludingOrganizationId.Value),
            cancellationToken);
    }
}
