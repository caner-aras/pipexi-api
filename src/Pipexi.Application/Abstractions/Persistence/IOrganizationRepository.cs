using Workforce.Domain.Entities;

namespace Workforce.Application.Abstractions.Persistence;

public interface IOrganizationRepository : IRepository<Organization>
{
    Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludingOrganizationId = null,
        CancellationToken cancellationToken = default);
}
