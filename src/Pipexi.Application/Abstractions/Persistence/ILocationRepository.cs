using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface ILocationRepository : IRepository<Location>
{
    Task<bool> NameExistsAsync(
        Guid organizationId,
        string name,
        Guid? excludingLocationId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Location>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
