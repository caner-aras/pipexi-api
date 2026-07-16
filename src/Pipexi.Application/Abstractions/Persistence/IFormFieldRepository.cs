using Workforce.Domain.Entities;

namespace Workforce.Application.Abstractions.Persistence;

public interface IFormFieldRepository : IRepository<FormField>
{
    Task<IReadOnlyCollection<FormField>> ListByFormTemplateIdAsync(
        Guid formTemplateId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<FormField>> ListByFormTemplateIdsAsync(
        IReadOnlyCollection<Guid> formTemplateIds,
        CancellationToken cancellationToken = default);
}
