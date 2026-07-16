using Workforce.Domain.Entities;

namespace Workforce.Application.Abstractions.Persistence;

public interface ILocationWorkingHourRepository : IRepository<LocationWorkingHour>
{
    Task<IReadOnlyCollection<LocationWorkingHour>> ListByLocationIdAsync(
        Guid locationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<LocationWorkingHour>> ListByLocationIdsAsync(
        IReadOnlyCollection<Guid> locationIds,
        CancellationToken cancellationToken = default);

    Task HardDeleteByLocationIdAsync(
        Guid locationId,
        CancellationToken cancellationToken = default);
}