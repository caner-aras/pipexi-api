using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface ITeamMemberRepository : IRepository<TeamMember>
{
    Task<bool> ExistsAsync(
        Guid teamId,
        Guid organizationMemberId,
        Guid? excludingTeamMemberId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeamMember>> ListByTeamIdAsync(
        Guid teamId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeamMember>> ListByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeamMember>> ListByOrganizationMemberIdsAsync(
        IReadOnlyCollection<Guid> organizationMemberIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> CountByTeamIdsAsync(
        IReadOnlyCollection<Guid> teamIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeamMember>> ListByTeamIdsAndOrganizationMemberIdsAsync(
        IReadOnlyCollection<Guid> teamIds,
        IReadOnlyCollection<Guid> organizationMemberIds,
        CancellationToken cancellationToken = default);
}
