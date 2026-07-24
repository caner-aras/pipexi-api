using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class PositionRepository(ApplicationDbContext dbContext) : Repository<Position>(dbContext), IPositionRepository
{
    public async Task<IReadOnlyCollection<Position>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByTitleAsync(
        Guid organizationId,
        string title,
        Guid? excludingPositionId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedTitle = title.Trim().ToLowerInvariant();

        return await DbSet.AnyAsync(x =>
            x.OrganizationId == organizationId &&
            x.Title.ToLower() == normalizedTitle &&
            (!excludingPositionId.HasValue || x.Id != excludingPositionId.Value),
            cancellationToken);
    }
}
