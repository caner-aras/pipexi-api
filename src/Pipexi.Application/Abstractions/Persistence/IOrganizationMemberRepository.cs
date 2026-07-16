using Workforce.Domain.Entities;

namespace Workforce.Application.Abstractions.Persistence;

public interface IOrganizationMemberRepository : IRepository<OrganizationMember>
{
    Task<OrganizationMember?> GetByOrganizationIdAndUserIdAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid organizationId,
        Guid userId,
        Guid? excludingOrganizationMemberId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OrganizationMember>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OrganizationMember>> ListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
