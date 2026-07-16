using Workforce.Domain.Entities;

namespace Workforce.Application.Abstractions.Persistence;

public interface ITimeEntryRepository : IRepository<TimeEntry>
{
    Task<IReadOnlyCollection<TimeEntry>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TimeEntry>> ListByOrganizationMemberIdAsync(
        Guid organizationMemberId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TimeEntry>> ListByShiftIdsAsync(
        IReadOnlyCollection<Guid> shiftIds,
        CancellationToken cancellationToken = default);
}
