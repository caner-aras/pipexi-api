using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Queries.GetStoredFiles;

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
