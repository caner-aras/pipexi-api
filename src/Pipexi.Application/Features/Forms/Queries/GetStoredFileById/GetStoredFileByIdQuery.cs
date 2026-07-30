using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Queries.GetStoredFileById;

public sealed record GetStoredFileByIdQuery(Guid Id, Guid? ScopedOrganizationId = null) : IQuery<Result<StoredFileDto>>
{
    public sealed class Handler : IRequestHandler<GetStoredFileByIdQuery, Result<StoredFileDto>>
    {
        private readonly IStoredFileRepository _storedFileRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(IStoredFileRepository storedFileRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationAccess = organizationAccess;
            _storedFileRepository = storedFileRepository;
        }

        public async Task<Result<StoredFileDto>> Handle(GetStoredFileByIdQuery request, CancellationToken cancellationToken)
        {
            var file = await _storedFileRepository.GetByIdAsync(request.Id, cancellationToken);
            if (file is null)
            {
                return Result<StoredFileDto>.Failure(
                    new AppError("files.not_found", "File not found."),
                    (int)HttpStatusCode.NotFound);
            }


            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<StoredFileDto>(
                file.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
            return Result<StoredFileDto>.Success(file.ToDto());
        }
    }
}
