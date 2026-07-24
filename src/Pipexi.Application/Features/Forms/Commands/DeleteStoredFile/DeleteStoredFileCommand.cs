using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Commands.DeleteStoredFile;

public sealed record DeleteStoredFileCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteStoredFileCommand, Result<object?>>
    {
        private readonly IStoredFileRepository _storedFileRepository;

        public Handler(IStoredFileRepository storedFileRepository)
        {
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

            await _storedFileRepository.DeleteAsync(file, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
