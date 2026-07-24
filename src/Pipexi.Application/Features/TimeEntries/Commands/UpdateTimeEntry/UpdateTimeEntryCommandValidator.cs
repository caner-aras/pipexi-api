using FluentValidation;

namespace Pipexi.Application.Features.TimeEntries.Commands.UpdateTimeEntry;

public sealed class UpdateTimeEntryCommandValidator : AbstractValidator<UpdateTimeEntryCommand>
{
    public UpdateTimeEntryCommandValidator()
    {
        RuleFor(x => x.ClockOutAt)
            .GreaterThan(x => x.ClockInAt)
            .When(x => x.ClockOutAt.HasValue && x.ClockInAt.HasValue)
            .WithMessage("ClockOutAt must be greater than ClockInAt.");

        RuleFor(x => x.EmployeeNote)
            .MaximumLength(2000)
            .When(x => x.EmployeeNote is not null);

        RuleFor(x => x.ManagerNote)
            .MaximumLength(2000)
            .When(x => x.ManagerNote is not null);
    }
}
