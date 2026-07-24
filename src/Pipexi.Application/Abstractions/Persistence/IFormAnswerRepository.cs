using Pipexi.Domain.Entities;

namespace Pipexi.Application.Abstractions.Persistence;

public interface IFormAnswerRepository : IRepository<FormAnswer>
{
    Task<IReadOnlyCollection<FormAnswer>> ListByFormSubmissionIdAsync(
        Guid formSubmissionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<FormAnswer>> ListByFormSubmissionIdsAsync(
        IReadOnlyCollection<Guid> formSubmissionIds,
        CancellationToken cancellationToken = default);
}
