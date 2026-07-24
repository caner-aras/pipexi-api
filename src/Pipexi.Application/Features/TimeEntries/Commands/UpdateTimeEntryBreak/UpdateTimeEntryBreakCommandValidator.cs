using FluentValidation;

namespace Pipexi.Application.Features.TimeEntries.Commands.UpdateTimeEntryBreak;

public sealed class UpdateTimeEntryBreakCommandValidator : AbstractValidator<UpdateTimeEntryBreakCommand>
{
    public UpdateTimeEntryBreakCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.StartAt.HasValue || !x.EndAt.HasValue || x.EndAt.Value > x.StartAt.Value)
            .WithMessage("EndAt must be greater than StartAt when both are provided.");
    }
}
