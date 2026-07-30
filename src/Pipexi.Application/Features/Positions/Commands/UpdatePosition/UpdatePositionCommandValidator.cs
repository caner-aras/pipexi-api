using FluentValidation;

namespace Pipexi.Application.Features.Positions.Commands.UpdatePosition;

public sealed class UpdatePositionCommandValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(150)
            .When(x => x.Title is not null);

        RuleFor(x => x.DefaultHourlyRate)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DefaultHourlyRate.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description is not null);

        RuleFor(x => x.Status)
            .MaximumLength(50)
            .When(x => x.Status is not null);
    }
}
