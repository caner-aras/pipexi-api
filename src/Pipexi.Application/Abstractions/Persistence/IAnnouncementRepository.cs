using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface IAnnouncementRepository : IRepository<Announcement>
{
    Task<IReadOnlyCollection<Announcement>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
