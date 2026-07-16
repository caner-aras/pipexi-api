using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Forms.Dtos;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Forms.Queries.GetStoredFiles;

public sealed record GetStoredFilesQuery(Guid? OrganizationId) : IQuery<Result<IReadOnlyCollection<StoredFileDto>>>
{
    public sealed class Handler : IRequestHandler<GetStoredFilesQuery, Result<IReadOnlyCollection<StoredFileDto>>>
    {
        private readonly IStoredFileRepository _storedFileRepository;

        public Handler(IStoredFileRepository storedFileRepository)
        {
            _storedFileRepository = storedFileRepository;
        }

        public async Task<Result<IReadOnlyCollection<StoredFileDto>>> Handle(GetStoredFilesQuery request, CancellationToken cancellationToken)
        {
            var files = request.OrganizationId.HasValue
                ? await _storedFileRepository.ListByOrganizationIdAsync(request.OrganizationId.Value, cancellationToken)
                : await _storedFileRepository.GetAllAsync(cancellationToken);

            var dtos = files.Select(x => x.ToDto()).ToList();
            return Result<IReadOnlyCollection<StoredFileDto>>.Success(dtos);
        }
    }
}
