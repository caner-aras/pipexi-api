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
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.OrganizationMemberId == organizationMemberId)
            .OrderByDescending(x => x.PaidAt)
            .ToListAsync(cancellationToken);
    }
}
