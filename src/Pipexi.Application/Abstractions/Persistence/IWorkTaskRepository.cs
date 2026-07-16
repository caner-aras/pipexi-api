using Workforce.Domain.Entities;

namespace Workforce.Application.Abstractions.Persistence;

public interface IWorkTaskRepository : IRepository<WorkTask>
{
    Task<IReadOnlyCollection<WorkTask>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<WorkTask>> ListByAssignedTeamMemberIdAsync(
        Guid teamMemberId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<WorkTask>> ListByAssignedTeamMemberIdsAsync(
        IReadOnlyCollection<Guid> teamMemberIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<WorkTask>> ListByAssignedTeamIdAsync(
        Guid teamId,
        CancellationToken cancellationToken = default);
}
