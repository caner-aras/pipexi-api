using FluentValidation;

namespace Pipexi.Application.Features.Forms.Commands.UpdateFormField;

public sealed class UpdateFormFieldCommandValidator : AbstractValidator<UpdateFormFieldCommand>
{
    public UpdateFormFieldCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Type)
            .MaximumLength(50)
            .When(x => x.Type is not null);

        RuleFor(x => x.Label)
            .MaximumLength(250)
            .When(x => x.Label is not null);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SortOrder.HasValue);

        RuleFor(x => x.OptionsJson)
            .MaximumLength(20000)
            .When(x => x.OptionsJson is not null);

        RuleFor(x => x.Status)
            .MaximumLength(30)
            .When(x => x.Status is not null);
    }
}
