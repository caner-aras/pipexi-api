using Workforce.Domain.Entities;

namespace Workforce.Application.Abstractions.Persistence;

public interface IShiftRepository : IRepository<Shift>
{
    Task<IReadOnlyCollection<Shift>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Shift>> ListByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Shift>> ListByTeamIdAsync(
        Guid teamId,
        CancellationToken cancellationToken = default);
}
