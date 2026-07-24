using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Positions.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Positions.Commands.CreatePosition;

public sealed record CreatePositionCommand(
    Guid OrganizationId,
    string Title,
    decimal DefaultHourlyRate,
    string? Description) : ICommand<Result<PositionDto>>
{
    public sealed class Handler(IPositionRepository positionRepository)
        : IRequestHandler<CreatePositionCommand, Result<PositionDto>>
    {
        public async Task<Result<PositionDto>> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
        {
            var exists = await positionRepository.ExistsByTitleAsync(
                request.OrganizationId,
                request.Title,
                cancellationToken: cancellationToken);

            if (exists)
            {
                return Result<PositionDto>.Failure(
                    new AppError("positions.title_conflict", "Position title already exists in this organization."),
                    (int)HttpStatusCode.Conflict);
            }

            var position = Position.Create(
                request.OrganizationId,
                request.Title,
                request.DefaultHourlyRate,
                request.Description);

            await positionRepository.AddAsync(position, cancellationToken);

            return Result<PositionDto>.Success(position.ToDto(), (int)HttpStatusCode.Created);
        }
    }
}
