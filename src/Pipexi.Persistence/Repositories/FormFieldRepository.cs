using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class FormFieldRepository : Repository<FormField>, IFormFieldRepository
{
    public FormFieldRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyCollection<FormField>> ListByFormTemplateIdAsync(
        Guid formTemplateId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.FormTemplateId == formTemplateId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<FormField>> ListByFormTemplateIdsAsync(
        IReadOnlyCollection<Guid> formTemplateIds,
        CancellationToken cancellationToken = default)
    {
        if (formTemplateIds.Count == 0)
        {
            return Array.Empty<FormField>();
        }

        return await DbSet
            .Where(x => formTemplateIds.Contains(x.FormTemplateId))
            .OrderBy(x => x.FormTemplateId)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
