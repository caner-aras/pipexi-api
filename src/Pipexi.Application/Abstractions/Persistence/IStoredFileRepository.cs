using Workforce.Domain.Entities;

namespace Workforce.Application.Abstractions.Persistence;

public interface IStoredFileRepository : IRepository<StoredFile>
{
    Task<IReadOnlyCollection<StoredFile>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
