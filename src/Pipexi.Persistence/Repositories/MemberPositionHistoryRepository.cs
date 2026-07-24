using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class MemberPositionHistoryRepository(ApplicationDbContext dbContext) : Repository<MemberPositionHistory>(dbContext), IMemberPositionHistoryRepository
{
    public async Task<MemberPositionHistory?> GetActiveByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.OrganizationMemberId == organizationMemberId && x.EndDate == null)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<MemberPositionHistory>> ListByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.OrganizationMemberId == organizationMemberId)
            .OrderByDescending(x => x.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<MemberPositionHistory?> GetByMemberAndDateAsync(
        Guid organizationMemberId,
        DateTimeOffset targetDate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.OrganizationMemberId == organizationMemberId &&
                        x.StartDate <= targetDate &&
                        (x.EndDate == null || x.EndDate > targetDate))
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
