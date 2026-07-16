using Microsoft.EntityFrameworkCore;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Domain.Entities;
using Workforce.Persistence.Context;

namespace Workforce.Persistence.Repositories;

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