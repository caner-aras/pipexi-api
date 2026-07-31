using FluentValidation;

namespace Pipexi.Application.Features.Teams.Commands.UpdateTeamMemberDayOff;

public sealed class UpdateTeamMemberDayOffCommandValidator : AbstractValidator<UpdateTeamMemberDayOffCommand>
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "pending",
        "active",
    };

    public UpdateTeamMemberDayOffCommandValidator()
    {
        RuleFor(x => x.DayOffId).NotEmpty();
        RuleFor(x => x.TeamMemberId).NotEmpty();

        RuleFor(x => x.StartAt)
            .Must(startAt => !startAt.HasValue || startAt.Value >= DateTimeOffset.UtcNow.AddMinutes(-1))
            .WithMessage("Day off start cannot be in the past.");

        RuleFor(x => x)
            .Must(x => !x.StartAt.HasValue || !x.EndAt.HasValue || x.EndAt > x.StartAt)
            .WithMessage("Day off end time must be after start time.");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => x.Reason is not null);

        RuleFor(x => x.Status)
            .Must(status => status is null || AllowedStatuses.Contains(status.Trim()))
            .WithMessage("Status must be pending or active.")
            .MaximumLength(50)
            .When(x => x.Status is not null);
    }
}
