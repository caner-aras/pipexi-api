using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Queries.GetTeamMemberDayOffById;

public sealed record GetTeamMemberDayOffByIdQuery(Guid Id, Guid TeamMemberId, Guid? ScopedOrganizationId = null) : IQuery<Result<TeamMemberDayOffDto>>;

public sealed class Handler : IRequestHandler<GetTeamMemberDayOffByIdQuery, Result<TeamMemberDayOffDto>>
{
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ITeamMemberDayOffRepository _teamMemberDayOffRepository;
    private readonly IOrganizationAccessService _organizationAccess;

    public Handler(
        ITeamMemberRepository teamMemberRepository,
        ITeamRepository teamRepository,
        ITeamMemberDayOffRepository teamMemberDayOffRepository,
        IOrganizationAccessService organizationAccess)
    {
        _teamMemberRepository = teamMemberRepository;
        _teamRepository = teamRepository;
        _teamMemberDayOffRepository = teamMemberDayOffRepository;
        _organizationAccess = organizationAccess;
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

        var teamMember = await _teamMemberRepository.GetByIdAsync(item.TeamMemberId, cancellationToken);
        if (teamMember is null)
        {
            return Result<TeamMemberDayOffDto>.Failure(
                new AppError("team_member_day_offs.invalid_team_member", "Team member not found."),
                (int)HttpStatusCode.BadRequest);
        }

        var team = await _teamRepository.GetByIdAsync(teamMember.TeamId, cancellationToken);
        if (team is null)
        {
            return Result<TeamMemberDayOffDto>.Failure(
                new AppError("teams.not_found", "Team not found."),
                (int)HttpStatusCode.BadRequest);
        }

        var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<TeamMemberDayOffDto>(
            team.OrganizationId, request.ScopedOrganizationId, cancellationToken);
        if (accessDenied is not null) return accessDenied;

        return Result<TeamMemberDayOffDto>.Success(item.ToDto());
    }
}
