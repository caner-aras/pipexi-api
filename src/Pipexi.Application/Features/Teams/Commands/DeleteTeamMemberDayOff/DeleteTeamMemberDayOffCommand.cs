using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Commands.DeleteTeamMemberDayOff;

public sealed record DeleteTeamMemberDayOffCommand(Guid DayOffId, Guid TeamMemberId) : ICommand<Result<bool>>;

public sealed class Handler : IRequestHandler<DeleteTeamMemberDayOffCommand, Result<bool>>
{
    private readonly ITeamMemberDayOffRepository _teamMemberDayOffRepository;

    public Handler(ITeamMemberDayOffRepository teamMemberDayOffRepository)
    {
        _teamMemberDayOffRepository = teamMemberDayOffRepository;
    }

    public async Task<Result<bool>> Handle(DeleteTeamMemberDayOffCommand request, CancellationToken cancellationToken)
    {
        var dayOff = await _teamMemberDayOffRepository.GetByIdAsync(request.DayOffId, cancellationToken);
        if (dayOff is null || dayOff.TeamMemberId != request.TeamMemberId)
        {
            return Result<bool>.Failure(new AppError("team_member_day_offs.not_found", "Team member day off not found."), 404);
        }

        await _teamMemberDayOffRepository.DeleteAsync(dayOff, cancellationToken);
        return Result<bool>.Success(true);
    }
}