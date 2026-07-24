using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.MemberPositions.Dtos;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.MemberPositions.Queries.GetMemberPositionHistory;

public sealed record GetMemberPositionHistoryQuery(Guid OrganizationMemberId)
    : IQuery<Result<IReadOnlyCollection<MemberPositionHistoryDto>>>
{
    public sealed class Handler(IMemberPositionHistoryRepository historyRepository)
        : IRequestHandler<GetMemberPositionHistoryQuery, Result<IReadOnlyCollection<MemberPositionHistoryDto>>>
    {
        public async Task<Result<IReadOnlyCollection<MemberPositionHistoryDto>>> Handle(
            GetMemberPositionHistoryQuery request,
            CancellationToken cancellationToken)
        {
            var history = await historyRepository.ListByOrganizationMemberIdAsync(
                request.OrganizationMemberId,
                cancellationToken);

            var dtos = history.Select(x => x.ToDto()).ToList();

            return Result<IReadOnlyCollection<MemberPositionHistoryDto>>.Success(dtos, (int)HttpStatusCode.OK);
        }
    }
}
