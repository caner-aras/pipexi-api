using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface ITeamMemberDayOffRepository : IRepository<TeamMemberDayOff>
{
    Task<IReadOnlyCollection<TeamMemberDayOff>> ListByTeamMemberIdAsync(
        Guid teamMemberId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeamMemberDayOff>> ListByTeamMemberIdsAsync(
        IReadOnlyCollection<Guid> teamMemberIds,
        CancellationToken cancellationToken = default);

    Task<bool> HasOverlapAsync(
        Guid teamMemberId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        Guid? excludingDayOffId = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasOverlapForTeamMembersAsync(
        IReadOnlyCollection<Guid> teamMemberIds,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeamMemberDayOff>> ListPendingByTeamMemberIdsAsync(
        IReadOnlyCollection<Guid> teamMemberIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeamMemberDayOff>> ListActiveByTeamMemberIdsAsync(
        IReadOnlyCollection<Guid> teamMemberIds,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);
}