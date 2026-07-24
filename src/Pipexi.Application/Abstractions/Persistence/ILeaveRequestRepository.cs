using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface ILeaveRequestRepository : IRepository<LeaveRequest>
{
    Task<IReadOnlyCollection<LeaveRequest>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<LeaveRequest>> ListByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        CancellationToken cancellationToken = default);
}
