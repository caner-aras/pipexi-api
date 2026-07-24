using FluentValidation;

namespace Pipexi.Application.Features.Shifts.Commands.UpdateShiftBreak;

public sealed class UpdateShiftBreakCommandValidator : AbstractValidator<UpdateShiftBreakCommand>
{
    public UpdateShiftBreakCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.StartAt.HasValue || !x.EndAt.HasValue || x.EndAt.Value > x.StartAt.Value)
            .WithMessage("EndAt must be greater than StartAt when both are provided.");
    }
}
