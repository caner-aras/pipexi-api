using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class ShiftRepository : Repository<Shift>, IShiftRepository
{
    public ShiftRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyCollection<Shift>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Shift>> ListByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.OrganizationMemberId == organizationMemberId)
            .OrderBy(x => x.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Shift>> ListByTeamIdAsync(
        Guid teamId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.TeamId == teamId)
            .OrderBy(x => x.StartAt)
            .ToListAsync(cancellationToken);
    }
}
