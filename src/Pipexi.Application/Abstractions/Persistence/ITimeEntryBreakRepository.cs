using Workforce.Domain.Entities;

namespace Workforce.Application.Abstractions.Persistence;

public interface ITimeEntryBreakRepository : IRepository<TimeEntryBreak>
{
    Task<IReadOnlyCollection<TimeEntryBreak>> ListByTimeEntryIdAsync(
        Guid timeEntryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TimeEntryBreak>> ListByTimeEntryIdsAsync(
        IReadOnlyCollection<Guid> timeEntryIds,
        CancellationToken cancellationToken = default);

    Task<bool> OverlapsAsync(
        Guid timeEntryId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        Guid? excludingTimeEntryBreakId = null,
        CancellationToken cancellationToken = default);
}
