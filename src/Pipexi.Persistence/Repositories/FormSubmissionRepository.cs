using Microsoft.EntityFrameworkCore;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;
using Pipexi.Persistence.Context;

namespace Pipexi.Persistence.Repositories;

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

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<Guid>>> ListSubmittedTemplateIdsByShiftIdsAsync(
        IReadOnlyCollection<Guid> shiftIds,
        CancellationToken cancellationToken = default)
    {
        if (shiftIds.Count == 0)
        {
            return new Dictionary<Guid, IReadOnlyCollection<Guid>>();
        }

        var ids = shiftIds.Distinct().ToList();
        var rows = await DbSet
            .Where(x => x.ShiftId.HasValue && ids.Contains(x.ShiftId.Value))
            .Select(x => new { ShiftId = x.ShiftId!.Value, x.FormTemplateId })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.ShiftId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyCollection<Guid>)g.Select(x => x.FormTemplateId).Distinct().ToList());
    }
}
