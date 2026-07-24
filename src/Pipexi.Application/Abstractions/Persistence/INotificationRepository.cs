using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IReadOnlyCollection<Notification>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Notification>> ListByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        CancellationToken cancellationToken = default);
}
