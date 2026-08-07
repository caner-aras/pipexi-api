using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface IOrganizationMemberPaymentRepository : IRepository<OrganizationMemberPayment>
{
    Task<IReadOnlyCollection<OrganizationMemberPayment>> ListByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        DateTimeOffset? fromPaidAt = null,
        DateTimeOffset? toPaidAtExclusive = null,
        CancellationToken cancellationToken = default);
}
