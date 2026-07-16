using Workforce.Domain.Entities;

namespace Workforce.Application.Abstractions.Persistence;

public interface IShiftBreakRepository : IRepository<ShiftBreak>
{
    Task<IReadOnlyCollection<ShiftBreak>> ListByShiftIdAsync(
        Guid shiftId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ShiftBreak>> ListByShiftIdsAsync(
        IReadOnlyCollection<Guid> shiftIds,
        CancellationToken cancellationToken = default);

    Task<bool> OverlapsAsync(
        Guid shiftId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        Guid? excludingShiftBreakId = null,
        CancellationToken cancellationToken = default);
}
