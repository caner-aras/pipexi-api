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
}