using Workforce.Domain.Entities;

namespace Workforce.Application.Abstractions.Persistence;

public interface IFormTemplateRepository : IRepository<FormTemplate>
{
    Task<IReadOnlyCollection<FormTemplate>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
