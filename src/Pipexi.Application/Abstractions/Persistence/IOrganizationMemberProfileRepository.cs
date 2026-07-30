using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface IOrganizationMemberProfileRepository : IRepository<OrganizationMemberProfile>
{
    Task<OrganizationMemberProfile?> GetByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        CancellationToken cancellationToken = default);
}
