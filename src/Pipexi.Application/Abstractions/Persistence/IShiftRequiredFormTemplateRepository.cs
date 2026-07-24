using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface IShiftRequiredFormTemplateRepository : IRepository<ShiftRequiredFormTemplate>
{
    Task<IReadOnlyCollection<ShiftRequiredFormTemplate>> ListByShiftIdAsync(
        Guid shiftId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Guid>> ListRequiredTemplateIdsByShiftIdAsync(
        Guid shiftId,
        CancellationToken cancellationToken = default);
}