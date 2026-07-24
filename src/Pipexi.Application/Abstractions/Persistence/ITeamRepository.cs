using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface ITeamRepository : IRepository<Team>
{
    Task<bool> NameExistsAsync(
        Guid organizationId,
        string name,
        Guid? excludingTeamId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Team>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
