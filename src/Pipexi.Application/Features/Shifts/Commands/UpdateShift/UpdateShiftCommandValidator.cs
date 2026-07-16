using FluentValidation;

namespace Workforce.Application.Features.Shifts.Commands.UpdateShift;

public sealed class UpdateShiftCommandValidator : AbstractValidator<UpdateShiftCommand>
{
    public UpdateShiftCommandValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => x.Title is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes is not null);

        RuleFor(x => x)
            .Must(x => !x.StartAt.HasValue || !x.EndAt.HasValue || x.EndAt.Value > x.StartAt.Value)
            .WithMessage("EndAt must be greater than StartAt when both are provided.");
    }
}
