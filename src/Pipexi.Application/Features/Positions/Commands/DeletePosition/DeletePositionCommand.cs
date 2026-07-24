using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Positions.Commands.DeletePosition;

public sealed record DeletePositionCommand(Guid Id) : ICommand<Result<bool>>
{
    public sealed class Handler(IPositionRepository positionRepository)
        : IRequestHandler<DeletePositionCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeletePositionCommand request, CancellationToken cancellationToken)
        {
            var position = await positionRepository.GetByIdAsync(request.Id, cancellationToken);
            if (position is null)
            {
                return Result<bool>.Failure(
                    new AppError("positions.not_found", "Position not found."),
                    (int)HttpStatusCode.NotFound);
            }

            position.MarkDeleted();
            await positionRepository.UpdateAsync(position, cancellationToken);

            return Result<bool>.Success(true, (int)HttpStatusCode.OK);
        }
    }
}
