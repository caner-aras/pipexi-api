using Workforce.Domain.Entities;

namespace Workforce.Application.Abstractions.Persistence;

public interface IRoleRepository : IRepository<Role>
{
    Task<bool> NameExistsAsync(
        Guid organizationId,
        string name,
        Guid? excludingRoleId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Role>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
