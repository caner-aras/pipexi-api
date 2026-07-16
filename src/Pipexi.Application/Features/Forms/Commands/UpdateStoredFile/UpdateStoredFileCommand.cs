using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Forms.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Forms.Commands.UpdateStoredFile;

public sealed record UpdateStoredFileCommand(
    Guid Id,
    string? FileName,
    string? ContentType,
    string? StoragePath,
    long? SizeBytes,
    string? Status) : ICommand<Result<StoredFileDto>>
{
    public sealed class Handler : IRequestHandler<UpdateStoredFileCommand, Result<StoredFileDto>>
    {
        private readonly IStoredFileRepository _storedFileRepository;

        public Handler(IStoredFileRepository storedFileRepository)
        {
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
