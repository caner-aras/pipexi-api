using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Queries.GetStoredFileById;

public sealed record GetStoredFileByIdQuery(Guid Id) : IQuery<Result<StoredFileDto>>
{
    public sealed class Handler : IRequestHandler<GetStoredFileByIdQuery, Result<StoredFileDto>>
    {
        private readonly IStoredFileRepository _storedFileRepository;

        public Handler(IStoredFileRepository storedFileRepository)
        {
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

            return Result<StoredFileDto>.Success(file.ToDto());
        }
    }
}
