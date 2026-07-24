using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Queries.GetTeamMemberDayOffById;

public sealed record GetTeamMemberDayOffByIdQuery(Guid Id, Guid TeamMemberId) : IQuery<Result<TeamMemberDayOffDto>>;

public sealed class Handler : IRequestHandler<GetTeamMemberDayOffByIdQuery, Result<TeamMemberDayOffDto>>
{
    private readonly ITeamMemberDayOffRepository _teamMemberDayOffRepository;

    public Handler(ITeamMemberDayOffRepository teamMemberDayOffRepository)
    {
        _teamMemberDayOffRepository = teamMemberDayOffRepository;
    }

    public async Task<Result<TeamMemberDayOffDto>> Handle(GetTeamMemberDayOffByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _teamMemberDayOffRepository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
        {
            return Result<TeamMemberDayOffDto>.Failure(
                new AppError("team_member_day_offs.not_found", "Team member day off not found."),
                (int)HttpStatusCode.NotFound);
        }

        if (item.TeamMemberId != request.TeamMemberId)
        {
            return Result<TeamMemberDayOffDto>.Failure(
                new AppError("team_member_day_offs.not_found", "Team member day off not found."),
                (int)HttpStatusCode.NotFound);
        }

        return Result<TeamMemberDayOffDto>.Success(item.ToDto());
    }
}