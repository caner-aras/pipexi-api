using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Positions.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Positions.Queries.GetPositionById;

public sealed record GetPositionByIdQuery(Guid Id) : IQuery<Result<PositionDto>>
{
    public sealed class Handler(IPositionRepository positionRepository)
        : IRequestHandler<GetPositionByIdQuery, Result<PositionDto>>
    {
        public async Task<Result<PositionDto>> Handle(
            GetPositionByIdQuery request,
            CancellationToken cancellationToken)
        {
            var position = await positionRepository.GetByIdAsync(request.Id, cancellationToken);
            if (position is null)
            {
                return Result<PositionDto>.Failure(
                    new AppError("positions.not_found", "Position not found."),
                    (int)HttpStatusCode.NotFound);
            }

            return Result<PositionDto>.Success(position.ToDto(), (int)HttpStatusCode.OK);
        }
    }
}
