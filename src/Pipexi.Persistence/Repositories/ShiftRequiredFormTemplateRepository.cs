using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class ShiftRequiredFormTemplateRepository : Repository<ShiftRequiredFormTemplate>, IShiftRequiredFormTemplateRepository
{
    public ShiftRequiredFormTemplateRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyCollection<ShiftRequiredFormTemplate>> ListByShiftIdAsync(
        Guid shiftId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.ShiftId == shiftId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> ListRequiredTemplateIdsByShiftIdAsync(
        Guid shiftId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.ShiftId == shiftId)
            .Select(x => x.FormTemplateId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<Guid>>> ListRequiredTemplateIdsByShiftIdsAsync(
        IReadOnlyCollection<Guid> shiftIds,
        CancellationToken cancellationToken = default)
    {
        if (shiftIds.Count == 0)
        {
            return new Dictionary<Guid, IReadOnlyCollection<Guid>>();
        }

        var ids = shiftIds.Distinct().ToList();
        var rows = await DbSet
            .Where(x => ids.Contains(x.ShiftId))
            .Select(x => new { x.ShiftId, x.FormTemplateId })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.ShiftId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyCollection<Guid>)g.Select(x => x.FormTemplateId).Distinct().ToList());
    }
}