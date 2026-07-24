using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Positions.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Positions.Commands.UpdatePosition;

public sealed record UpdatePositionCommand(
    Guid Id,
    string? Title,
    decimal? DefaultHourlyRate,
    string? Description,
    string? Status) : ICommand<Result<PositionDto>>
{
    public sealed class Handler(IPositionRepository positionRepository)
        : IRequestHandler<UpdatePositionCommand, Result<PositionDto>>
    {
        public async Task<Result<PositionDto>> Handle(UpdatePositionCommand request, CancellationToken cancellationToken)
        {
            var position = await positionRepository.GetByIdAsync(request.Id, cancellationToken);
            if (position is null)
            {
                return Result<PositionDto>.Failure(
                    new AppError("positions.not_found", "Position not found."),
                    (int)HttpStatusCode.NotFound);
            }

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                var titleExists = await positionRepository.ExistsByTitleAsync(
                    position.OrganizationId,
                    request.Title,
                    excludingPositionId: position.Id,
                    cancellationToken: cancellationToken);

                if (titleExists)
                {
                    return Result<PositionDto>.Failure(
                        new AppError("positions.title_conflict", "Position title already exists in this organization."),
                        (int)HttpStatusCode.Conflict);
                }
            }

            position.UpdateDetails(request.Title, request.Description, request.DefaultHourlyRate, request.Status);
            await positionRepository.UpdateAsync(position, cancellationToken);

            return Result<PositionDto>.Success(position.ToDto(), (int)HttpStatusCode.OK);
        }
    }
}
