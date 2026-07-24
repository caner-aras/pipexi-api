using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface IPositionRepository : IRepository<Position>
{
    Task<IReadOnlyCollection<Position>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByTitleAsync(
        Guid organizationId,
        string title,
        Guid? excludingPositionId = null,
        CancellationToken cancellationToken = default);
}
