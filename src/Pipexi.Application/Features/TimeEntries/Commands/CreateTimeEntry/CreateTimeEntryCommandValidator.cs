using FluentValidation;

namespace Workforce.Application.Features.TimeEntries.Commands.CreateTimeEntry;

public sealed class CreateTimeEntryCommandValidator : AbstractValidator<CreateTimeEntryCommand>
{
    public CreateTimeEntryCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.OrganizationMemberId).NotEmpty();
        RuleFor(x => x.LocationId).NotEmpty();

        RuleFor(x => x.ClockOutAt)
            .GreaterThan(x => x.ClockInAt)
            .When(x => x.ClockOutAt.HasValue)
            .WithMessage("ClockOutAt must be greater than ClockInAt.");

        RuleFor(x => x.EmployeeNote)
            .MaximumLength(2000)
            .When(x => x.EmployeeNote is not null);

        RuleFor(x => x.ManagerNote)
            .MaximumLength(2000)
            .When(x => x.ManagerNote is not null);

        RuleForEach(x => x.Breaks).ChildRules(breakRules =>
        {
            breakRules.RuleFor(x => x.EndAt)
                .GreaterThan(x => x.StartAt)
                .WithMessage("Break EndAt must be greater than StartAt.");
        });
    }
}
