using System.Net;
using MediatR;
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
    string? Reason) : ICommand<Result<TeamMemberDayOffDto>>;

public sealed class Handler : IRequestHandler<CreateTeamMemberDayOffCommand, Result<TeamMemberDayOffDto>>
{
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly ITeamMemberDayOffRepository _teamMemberDayOffRepository;

    public Handler(
        ITeamMemberRepository teamMemberRepository,
        ITeamMemberDayOffRepository teamMemberDayOffRepository)
    {
        _teamMemberRepository = teamMemberRepository;
        _teamMemberDayOffRepository = teamMemberDayOffRepository;
    }

    public async Task<Result<TeamMemberDayOffDto>> Handle(CreateTeamMemberDayOffCommand request, CancellationToken cancellationToken)
    {
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

        var dayOff = TeamMemberDayOff.Create(
            request.TeamMemberId,
            request.StartAt,
            request.EndAt,
            request.Reason);

        await _teamMemberDayOffRepository.AddAsync(dayOff, cancellationToken);

        return Result<TeamMemberDayOffDto>.Success(dayOff.ToDto(), (int)HttpStatusCode.Created);
    }
}