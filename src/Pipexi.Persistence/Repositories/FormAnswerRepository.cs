using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

public sealed class FormAnswerRepository : Repository<FormAnswer>, IFormAnswerRepository
{
    public FormAnswerRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyCollection<FormAnswer>> ListByFormSubmissionIdAsync(
        Guid formSubmissionId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.FormSubmissionId == formSubmissionId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<FormAnswer>> ListByFormSubmissionIdsAsync(
        IReadOnlyCollection<Guid> formSubmissionIds,
        CancellationToken cancellationToken = default)
    {
        if (formSubmissionIds.Count == 0)
        {
            return Array.Empty<FormAnswer>();
        }

        return await DbSet
            .Where(x => formSubmissionIds.Contains(x.FormSubmissionId))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
