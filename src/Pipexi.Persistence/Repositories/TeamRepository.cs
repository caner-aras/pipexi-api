using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class TeamRepository : Repository<Team>, ITeamRepository
{
    public TeamRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<bool> NameExistsAsync(
        Guid organizationId,
        string name,
        Guid? excludingTeamId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLowerInvariant();

        return await DbSet.AnyAsync(x =>
            x.OrganizationId == organizationId &&
            x.Name.ToLower() == normalizedName &&
            (!excludingTeamId.HasValue || x.Id != excludingTeamId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<Team>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
