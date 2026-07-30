using FluentValidation;

namespace Pipexi.Application.Features.Teams.Commands.CreateTeamMemberDayOff;

public sealed class CreateTeamMemberDayOffCommandValidator : AbstractValidator<CreateTeamMemberDayOffCommand>
{
    public CreateTeamMemberDayOffCommandValidator()
    {
        RuleFor(x => x.TeamMemberId).NotEmpty();

        RuleFor(x => x.EndAt)
            .GreaterThan(x => x.StartAt)
            .WithMessage("Day off end time must be after start time.");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => x.Reason is not null);
    }
}
