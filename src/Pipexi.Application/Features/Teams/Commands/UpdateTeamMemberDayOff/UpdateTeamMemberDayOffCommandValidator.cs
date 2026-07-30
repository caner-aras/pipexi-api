using FluentValidation;

namespace Pipexi.Application.Features.Teams.Commands.UpdateTeamMemberDayOff;

public sealed class UpdateTeamMemberDayOffCommandValidator : AbstractValidator<UpdateTeamMemberDayOffCommand>
{
    public UpdateTeamMemberDayOffCommandValidator()
    {
        RuleFor(x => x.DayOffId).NotEmpty();
        RuleFor(x => x.TeamMemberId).NotEmpty();

        RuleFor(x => x)
            .Must(x => !x.StartAt.HasValue || !x.EndAt.HasValue || x.EndAt > x.StartAt)
            .WithMessage("Day off end time must be after start time.");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => x.Reason is not null);

        RuleFor(x => x.Status)
            .MaximumLength(50)
            .When(x => x.Status is not null);
    }
}
