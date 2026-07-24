using FluentValidation;

namespace Pipexi.Application.Features.Shifts.Commands.CreateShiftBreak;

public sealed class CreateShiftBreakCommandValidator : AbstractValidator<CreateShiftBreakCommand>
{
    public CreateShiftBreakCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();

        RuleFor(x => x.EndAt)
            .GreaterThan(x => x.StartAt)
            .WithMessage("EndAt must be greater than StartAt.");
    }
}
