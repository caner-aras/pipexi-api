using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Commands.UpdateTeamMemberDayOff;

public sealed record UpdateTeamMemberDayOffCommand(
    Guid DayOffId,
    Guid TeamMemberId,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt,
    string? Reason,
    string? Status,
    Guid? ScopedOrganizationId = null) : ICommand<Result<TeamMemberDayOffDto>>;

public sealed class Handler : IRequestHandler<UpdateTeamMemberDayOffCommand, Result<TeamMemberDayOffDto>>
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

    public async Task<Result<TeamMemberDayOffDto>> Handle(UpdateTeamMemberDayOffCommand request, CancellationToken cancellationToken)
    {
        var dayOff = await _teamMemberDayOffRepository.GetByIdAsync(request.DayOffId, cancellationToken);
        if (dayOff is null)
        {
            return Result<TeamMemberDayOffDto>.Failure(
                new AppError("team_member_day_offs.not_found", "Team member day off not found."),
                (int)HttpStatusCode.NotFound);
        }

        if (dayOff.TeamMemberId != request.TeamMemberId)
        {
            return Result<TeamMemberDayOffDto>.Failure(
                new AppError("team_member_day_offs.not_found", "Team member day off not found."),
                (int)HttpStatusCode.NotFound);
        }

        var teamMember = await _teamMemberRepository.GetByIdAsync(dayOff.TeamMemberId, cancellationToken);
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

        var nextStartAt = request.StartAt ?? dayOff.StartAt;
        var nextEndAt = request.EndAt ?? dayOff.EndAt;
        if (nextStartAt >= nextEndAt)
        {
            return Result<TeamMemberDayOffDto>.Failure(
                new AppError("team_member_day_offs.invalid_range", "Day off end time must be after start time."),
                (int)HttpStatusCode.BadRequest);
        }

        var hasOverlap = await _teamMemberDayOffRepository.HasOverlapAsync(
            dayOff.TeamMemberId,
            nextStartAt,
            nextEndAt,
            request.DayOffId,
            cancellationToken);

        if (hasOverlap)
        {
            return Result<TeamMemberDayOffDto>.Failure(
                new AppError("team_member_day_offs.overlap", "Day off overlaps with an existing record."),
                (int)HttpStatusCode.Conflict);
        }

        dayOff.UpdateDetails(request.StartAt, request.EndAt, request.Reason, request.Status);
        await _teamMemberDayOffRepository.UpdateAsync(dayOff, cancellationToken);

        return Result<TeamMemberDayOffDto>.Success(dayOff.ToDto());
    }
}
