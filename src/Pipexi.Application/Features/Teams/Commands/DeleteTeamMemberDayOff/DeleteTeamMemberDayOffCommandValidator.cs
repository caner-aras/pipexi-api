using FluentValidation;

namespace Pipexi.Application.Features.Teams.Commands.DeleteTeamMemberDayOff;

public sealed class DeleteTeamMemberDayOffCommandValidator : AbstractValidator<DeleteTeamMemberDayOffCommand>
{
    public DeleteTeamMemberDayOffCommandValidator()
    {
        RuleFor(x => x.DayOffId).NotEmpty();
        RuleFor(x => x.TeamMemberId).NotEmpty();
    }
}
