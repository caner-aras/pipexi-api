using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface IOrganizationRepository : IRepository<Organization>
{
    Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludingOrganizationId = null,
        CancellationToken cancellationToken = default);
}
