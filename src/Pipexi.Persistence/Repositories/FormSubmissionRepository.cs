using Microsoft.EntityFrameworkCore;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Domain.Entities;
using Workforce.Persistence.Context;

namespace Workforce.Persistence.Repositories;

public sealed class FormSubmissionRepository : Repository<FormSubmission>, IFormSubmissionRepository
{
    public FormSubmissionRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyCollection<FormSubmission>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<FormSubmission>> ListByFormTemplateIdAsync(
        Guid formTemplateId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.FormTemplateId == formTemplateId)
            .OrderByDescending(x => x.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> ListSubmittedTemplateIdsByShiftAsync(
        Guid shiftId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.ShiftId == shiftId)
            .Select(x => x.FormTemplateId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> ListSubmittedTemplateIdsByShiftAndMemberAsync(
        Guid shiftId,
        Guid organizationMemberId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.ShiftId == shiftId && x.SubmittedByMemberId == organizationMemberId)
            .Select(x => x.FormTemplateId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
