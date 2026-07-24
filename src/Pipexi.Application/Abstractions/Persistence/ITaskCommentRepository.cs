using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface ITaskCommentRepository : IRepository<TaskComment>
{
    Task<IReadOnlyCollection<TaskComment>> ListByWorkTaskIdAsync(
        Guid workTaskId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TaskComment>> ListByWorkTaskIdsAsync(
        IReadOnlyCollection<Guid> workTaskIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TaskComment>> ListByTeamMemberIdsAsync(
        IReadOnlyCollection<Guid> teamMemberIds,
        CancellationToken cancellationToken = default);
}
