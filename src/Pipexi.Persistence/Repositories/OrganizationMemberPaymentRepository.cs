using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class OrganizationMemberPaymentRepository(ApplicationDbContext dbContext)
    : Repository<OrganizationMemberPayment>(dbContext), IOrganizationMemberPaymentRepository
{
    public async Task<IReadOnlyCollection<OrganizationMemberPayment>> ListByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        DateTimeOffset? fromPaidAt = null,
        DateTimeOffset? toPaidAtExclusive = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(x => x.OrganizationMemberId == organizationMemberId);

        if (fromPaidAt.HasValue)
        {
            query = query.Where(x => x.PaidAt >= fromPaidAt.Value);
        }

        if (toPaidAtExclusive.HasValue)
        {
            query = query.Where(x => x.PaidAt < toPaidAtExclusive.Value);
        }

        return await query
            .OrderByDescending(x => x.PaidAt)
            .ToListAsync(cancellationToken);
    }
}
