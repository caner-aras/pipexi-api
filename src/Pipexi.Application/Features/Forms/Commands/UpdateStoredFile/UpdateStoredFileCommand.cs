using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Commands.UpdateStoredFile;

public sealed record UpdateStoredFileCommand(
    Guid Id,
    string? FileName,
    string? ContentType,
    string? StoragePath,
    long? SizeBytes,
    string? Status, Guid? ScopedOrganizationId = null) : ICommand<Result<StoredFileDto>>
{
    public sealed class Handler : IRequestHandler<UpdateStoredFileCommand, Result<StoredFileDto>>
    {
        private readonly IStoredFileRepository _storedFileRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(IStoredFileRepository storedFileRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationAccess = organizationAccess;
            _storedFileRepository = storedFileRepository;
        }

        public async Task<Result<StoredFileDto>> Handle(UpdateStoredFileCommand request, CancellationToken cancellationToken)
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
            file.UpdateDetails(
                request.FileName,
                request.ContentType,
                request.StoragePath,
                request.SizeBytes,
                request.Status);

            await _storedFileRepository.UpdateAsync(file, cancellationToken);
            return Result<StoredFileDto>.Success(file.ToDto(), (int)HttpStatusCode.OK);
        }
    }
}
