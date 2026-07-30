using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface IOrganizationMemberPaymentRepository : IRepository<OrganizationMemberPayment>
{
    Task<IReadOnlyCollection<OrganizationMemberPayment>> ListByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        CancellationToken cancellationToken = default);
}
