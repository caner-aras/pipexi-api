using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Commands.CreateTeamMemberDayOff;

public sealed record CreateTeamMemberDayOffCommand(
    Guid TeamMemberId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string? Reason,
    Guid? ScopedOrganizationId = null) : ICommand<Result<TeamMemberDayOffDto>>;

public sealed class Handler : IRequestHandler<CreateTeamMemberDayOffCommand, Result<TeamMemberDayOffDto>>
{
    private static readonly HashSet<string> MemberRequesterRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "member",
        "staff",
        "employee",
        "user",
    };

    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ITeamMemberDayOffRepository _teamMemberDayOffRepository;
    private readonly IOrganizationAccessService _organizationAccess;
    private readonly ICurrentUserContext _currentUserContext;

    public Handler(
        ITeamMemberRepository teamMemberRepository,
        ITeamRepository teamRepository,
        ITeamMemberDayOffRepository teamMemberDayOffRepository,
        IOrganizationAccessService organizationAccess,
        ICurrentUserContext currentUserContext)
    {
        _teamMemberRepository = teamMemberRepository;
        _teamRepository = teamRepository;
        _teamMemberDayOffRepository = teamMemberDayOffRepository;
        _organizationAccess = organizationAccess;
        _currentUserContext = currentUserContext;
    }

    public async Task<Result<TeamMemberDayOffDto>> Handle(CreateTeamMemberDayOffCommand request, CancellationToken cancellationToken)
    {
        if (request.StartAt < DateTimeOffset.UtcNow.AddMinutes(-1))
        {
            return Result<TeamMemberDayOffDto>.Failure(
                new AppError("team_member_day_offs.past_start", "Day off start cannot be in the past."),
                (int)HttpStatusCode.BadRequest);
        }

        if (request.StartAt >= request.EndAt)
        {
            return Result<TeamMemberDayOffDto>.Failure(
                new AppError("team_member_day_offs.invalid_range", "Day off end time must be after start time."),
                (int)HttpStatusCode.BadRequest);
        }

        var teamMember = await _teamMemberRepository.GetByIdAsync(request.TeamMemberId, cancellationToken);
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

        var hasOverlap = await _teamMemberDayOffRepository.HasOverlapAsync(
            request.TeamMemberId,
            request.StartAt,
            request.EndAt,
            cancellationToken: cancellationToken);

        if (hasOverlap)
        {
            return Result<TeamMemberDayOffDto>.Failure(
                new AppError("team_member_day_offs.overlap", "Day off overlaps with an existing record."),
                (int)HttpStatusCode.Conflict);
        }

        var status = IsMemberRequester(_currentUserContext.Role) ? "pending" : "active";

        var dayOff = TeamMemberDayOff.Create(
            request.TeamMemberId,
            request.StartAt,
            request.EndAt,
            request.Reason,
            status);

        await _teamMemberDayOffRepository.AddAsync(dayOff, cancellationToken);

        return Result<TeamMemberDayOffDto>.Success(dayOff.ToDto(), (int)HttpStatusCode.Created);
    }

    private static bool IsMemberRequester(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        return MemberRequesterRoles.Contains(role.Trim());
    }
}
