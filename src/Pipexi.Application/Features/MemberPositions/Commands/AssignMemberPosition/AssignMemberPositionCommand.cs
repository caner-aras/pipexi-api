using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.MemberPositions.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.MemberPositions.Commands.AssignMemberPosition;

public sealed record AssignMemberPositionCommand(
    Guid OrganizationMemberId,
    Guid PositionId,
    decimal HourlyRate,
    DateTimeOffset? StartDate = null) : ICommand<Result<MemberPositionHistoryDto>>
{
    public sealed class Handler(
        IMemberPositionHistoryRepository historyRepository,
        IPositionRepository positionRepository,
        IOrganizationMemberRepository memberRepository)
        : IRequestHandler<AssignMemberPositionCommand, Result<MemberPositionHistoryDto>>
    {
        public async Task<Result<MemberPositionHistoryDto>> Handle(
            AssignMemberPositionCommand request,
            CancellationToken cancellationToken)
        {
            var memberExists = await memberRepository.GetByIdAsync(request.OrganizationMemberId, cancellationToken);
            if (memberExists is null)
            {
                return Result<MemberPositionHistoryDto>.Failure(
                    new AppError("organization_members.not_found", "Organization member not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var positionExists = await positionRepository.GetByIdAsync(request.PositionId, cancellationToken);
            if (positionExists is null)
            {
                return Result<MemberPositionHistoryDto>.Failure(
                    new AppError("positions.not_found", "Position not found."),
                    (int)HttpStatusCode.NotFound);
            }

            // Close existing active position assignment if present
            var activeAssignment = await historyRepository.GetActiveByOrganizationMemberIdAsync(
                request.OrganizationMemberId,
                cancellationToken);

            var effectiveStartDate = request.StartDate ?? DateTimeOffset.UtcNow;

            if (activeAssignment is not null)
            {
                activeAssignment.EndAssignment(effectiveStartDate);
                await historyRepository.UpdateAsync(activeAssignment, cancellationToken);
            }

            var newAssignment = MemberPositionHistory.Create(
                request.OrganizationMemberId,
                request.PositionId,
                request.HourlyRate,
                effectiveStartDate);

            await historyRepository.AddAsync(newAssignment, cancellationToken);

            return Result<MemberPositionHistoryDto>.Success(newAssignment.ToDto(), (int)HttpStatusCode.Created);
        }
    }
}
