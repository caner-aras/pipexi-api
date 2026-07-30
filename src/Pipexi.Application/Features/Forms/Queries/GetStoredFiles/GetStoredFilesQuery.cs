using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Queries.GetStoredFiles;

public sealed record GetStoredFilesQuery(Guid? OrganizationId) : IQuery<Result<IReadOnlyCollection<StoredFileDto>>>
{
    public sealed class Handler : IRequestHandler<GetStoredFilesQuery, Result<IReadOnlyCollection<StoredFileDto>>>
    {
        private readonly IStoredFileRepository _storedFileRepository;
        private readonly ICurrentUserContext _currentUserContext;

        public Handler(IStoredFileRepository storedFileRepository, ICurrentUserContext currentUserContext)
        {
            _storedFileRepository = storedFileRepository;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<IReadOnlyCollection<StoredFileDto>>> Handle(GetStoredFilesQuery request, CancellationToken cancellationToken)
        {
            var organizationId = request.OrganizationId ?? _currentUserContext.OrganizationId;
            if (organizationId == Guid.Empty)
            {
                return Result<IReadOnlyCollection<StoredFileDto>>.Failure(
                    new AppError("auth.organization_required", "Organization is required."),
                    (int)HttpStatusCode.Forbidden);
            }

            var files = await _storedFileRepository.ListByOrganizationIdAsync(organizationId, cancellationToken);

            var dtos = files.Select(x => x.ToDto()).ToList();
            return Result<IReadOnlyCollection<StoredFileDto>>.Success(dtos);
        }
    }
}
