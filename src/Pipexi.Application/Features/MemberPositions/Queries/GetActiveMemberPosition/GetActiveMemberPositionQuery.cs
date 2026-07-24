using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.MemberPositions.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.MemberPositions.Queries.GetActiveMemberPosition;

public sealed record GetActiveMemberPositionQuery(Guid OrganizationMemberId) : IQuery<Result<MemberPositionHistoryDto>>
{
    public sealed class Handler(IMemberPositionHistoryRepository historyRepository)
        : IRequestHandler<GetActiveMemberPositionQuery, Result<MemberPositionHistoryDto>>
    {
        public async Task<Result<MemberPositionHistoryDto>> Handle(
            GetActiveMemberPositionQuery request,
            CancellationToken cancellationToken)
        {
            var activeAssignment = await historyRepository.GetActiveByOrganizationMemberIdAsync(
                request.OrganizationMemberId,
                cancellationToken);

            if (activeAssignment is null)
            {
                return Result<MemberPositionHistoryDto>.Failure(
                    new AppError("member_positions.active_not_found", "No active position assignment found for this member."),
                    (int)HttpStatusCode.NotFound);
            }

            return Result<MemberPositionHistoryDto>.Success(activeAssignment.ToDto(), (int)HttpStatusCode.OK);
        }
    }
}
