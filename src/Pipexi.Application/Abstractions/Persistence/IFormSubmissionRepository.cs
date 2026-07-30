using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface IFormSubmissionRepository : IRepository<FormSubmission>
{
    Task<IReadOnlyCollection<FormSubmission>> ListByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<FormSubmission>> ListByFormTemplateIdAsync(
        Guid formTemplateId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Guid>> ListSubmittedTemplateIdsByShiftAsync(
        Guid shiftId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Guid>> ListSubmittedTemplateIdsByShiftAndMemberAsync(
        Guid shiftId,
        Guid organizationMemberId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<Guid>>> ListSubmittedTemplateIdsByShiftIdsAsync(
        IReadOnlyCollection<Guid> shiftIds,
        CancellationToken cancellationToken = default);
}
