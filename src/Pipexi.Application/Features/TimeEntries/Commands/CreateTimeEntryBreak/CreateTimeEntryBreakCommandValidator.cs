using FluentValidation;

namespace Pipexi.Application.Features.TimeEntries.Commands.CreateTimeEntryBreak;

public sealed class CreateTimeEntryBreakCommandValidator : AbstractValidator<CreateTimeEntryBreakCommand>
{
    public CreateTimeEntryBreakCommandValidator()
    {
        RuleFor(x => x.TimeEntryId).NotEmpty();

        RuleFor(x => x.EndAt)
            .GreaterThan(x => x.StartAt)
            .WithMessage("EndAt must be greater than StartAt.");
    }
}
