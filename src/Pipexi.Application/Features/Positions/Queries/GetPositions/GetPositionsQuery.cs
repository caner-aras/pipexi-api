using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Positions.Dtos;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Positions.Queries.GetPositions;

public sealed record GetPositionsQuery(Guid OrganizationId) : IQuery<Result<IReadOnlyCollection<PositionDto>>>
{
    public sealed class Handler(IPositionRepository positionRepository)
        : IRequestHandler<GetPositionsQuery, Result<IReadOnlyCollection<PositionDto>>>
    {
        public async Task<Result<IReadOnlyCollection<PositionDto>>> Handle(
            GetPositionsQuery request,
            CancellationToken cancellationToken)
        {
            var positions = await positionRepository.ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
            var dtos = positions.Select(x => x.ToDto()).ToList();

            return Result<IReadOnlyCollection<PositionDto>>.Success(dtos, (int)HttpStatusCode.OK);
        }
    }
}
