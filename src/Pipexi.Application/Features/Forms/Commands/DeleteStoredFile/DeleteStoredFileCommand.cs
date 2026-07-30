using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Commands.DeleteStoredFile;

public sealed record DeleteStoredFileCommand(Guid Id, Guid? ScopedOrganizationId = null) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteStoredFileCommand, Result<object?>>
    {
        private readonly IStoredFileRepository _storedFileRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(IStoredFileRepository storedFileRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationAccess = organizationAccess;
            _storedFileRepository = storedFileRepository;
        }

        public async Task<Result<object?>> Handle(DeleteStoredFileCommand request, CancellationToken cancellationToken)
        {
            var file = await _storedFileRepository.GetByIdAsync(request.Id, cancellationToken);
            if (file is null)
            {
                return Result<object?>.Failure(
                    new AppError("files.not_found", "File not found."),
                    (int)HttpStatusCode.NotFound);
            }


            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<object?>(
                file.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
            await _storedFileRepository.DeleteAsync(file, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
