using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Forms.Dtos;
using Workforce.Domain.Entities;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Forms.Commands.CreateStoredFile;

public sealed record CreateStoredFileCommand(
    Guid OrganizationId,
    string FileName,
    string ContentType,
    string StoragePath,
    long SizeBytes) : ICommand<Result<StoredFileDto>>
{
    public sealed class Handler : IRequestHandler<CreateStoredFileCommand, Result<StoredFileDto>>
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IStoredFileRepository _storedFileRepository;

        public Handler(IOrganizationRepository organizationRepository, IStoredFileRepository storedFileRepository)
        {
            _organizationRepository = organizationRepository;
            _storedFileRepository = storedFileRepository;
        }

        public async Task<Result<StoredFileDto>> Handle(CreateStoredFileCommand request, CancellationToken cancellationToken)
        {
            var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
            if (organization is null)
            {
                return Result<StoredFileDto>.Failure(
                    new AppError("files.invalid_organization", "Organization not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var file = StoredFile.Create(
                request.OrganizationId,
                request.FileName,
                request.ContentType,
                request.StoragePath,
                request.SizeBytes);

            await _storedFileRepository.AddAsync(file, cancellationToken);
            return Result<StoredFileDto>.Success(file.ToDto(), (int)HttpStatusCode.Created);
        }
    }
}
