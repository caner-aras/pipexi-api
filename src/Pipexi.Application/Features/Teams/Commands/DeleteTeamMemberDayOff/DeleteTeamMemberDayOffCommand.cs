using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Commands.DeleteTeamMemberDayOff;

public sealed record DeleteTeamMemberDayOffCommand(Guid DayOffId, Guid TeamMemberId, Guid? ScopedOrganizationId = null) : ICommand<Result<bool>>;

public sealed class Handler : IRequestHandler<DeleteTeamMemberDayOffCommand, Result<bool>>
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

    public async Task<Result<bool>> Handle(DeleteTeamMemberDayOffCommand request, CancellationToken cancellationToken)
    {
        var dayOff = await _teamMemberDayOffRepository.GetByIdAsync(request.DayOffId, cancellationToken);
        if (dayOff is null || dayOff.TeamMemberId != request.TeamMemberId)
        {
            return Result<bool>.Failure(new AppError("team_member_day_offs.not_found", "Team member day off not found."), 404);
        }

        var teamMember = await _teamMemberRepository.GetByIdAsync(dayOff.TeamMemberId, cancellationToken);
        if (teamMember is null)
        {
            return Result<bool>.Failure(
                new AppError("team_member_day_offs.invalid_team_member", "Team member not found."),
                (int)HttpStatusCode.BadRequest);
        }

        var team = await _teamRepository.GetByIdAsync(teamMember.TeamId, cancellationToken);
        if (team is null)
        {
            return Result<bool>.Failure(
                new AppError("teams.not_found", "Team not found."),
                (int)HttpStatusCode.BadRequest);
        }

        var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<bool>(
            team.OrganizationId, request.ScopedOrganizationId, cancellationToken);
        if (accessDenied is not null) return accessDenied;

        await _teamMemberDayOffRepository.DeleteAsync(dayOff, cancellationToken);
        return Result<bool>.Success(true);
    }
}
