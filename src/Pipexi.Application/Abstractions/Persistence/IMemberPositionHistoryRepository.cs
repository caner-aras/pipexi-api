using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface IMemberPositionHistoryRepository : IRepository<MemberPositionHistory>
{
    Task<MemberPositionHistory?> GetActiveByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MemberPositionHistory>> ListByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        CancellationToken cancellationToken = default);

    Task<MemberPositionHistory?> GetByMemberAndDateAsync(
        Guid organizationMemberId,
        DateTimeOffset targetDate,
        CancellationToken cancellationToken = default);
}
