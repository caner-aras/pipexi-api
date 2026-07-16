using Microsoft.EntityFrameworkCore;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Domain.Entities;
using Workforce.Persistence.Context;

namespace Workforce.Persistence.Repositories;

public sealed class ShiftBreakRepository : Repository<ShiftBreak>, IShiftBreakRepository
{
    public ShiftBreakRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyCollection<ShiftBreak>> ListByShiftIdAsync(
        Guid shiftId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.ShiftId == shiftId)
            .OrderBy(x => x.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ShiftBreak>> ListByShiftIdsAsync(
        IReadOnlyCollection<Guid> shiftIds,
        CancellationToken cancellationToken = default)
    {
        if (shiftIds.Count == 0)
        {
            return Array.Empty<ShiftBreak>();
        }

        return await DbSet
            .Where(x => shiftIds.Contains(x.ShiftId))
            .OrderBy(x => x.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> OverlapsAsync(
        Guid shiftId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        Guid? excludingShiftBreakId = null,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(x =>
            x.ShiftId == shiftId &&
            (!excludingShiftBreakId.HasValue || x.Id != excludingShiftBreakId.Value) &&
            startAt < x.EndAt &&
            endAt > x.StartAt,
            cancellationToken);
    }
}
