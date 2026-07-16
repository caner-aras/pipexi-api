using FluentValidation;

namespace Workforce.Application.Features.Forms.Commands.CreateFormField;

public sealed class CreateFormFieldCommandValidator : AbstractValidator<CreateFormFieldCommand>
{
    public CreateFormFieldCommandValidator()
    {
        RuleFor(x => x.FormTemplateId).NotEmpty();

        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Label)
            .NotEmpty()
            .MaximumLength(250);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.OptionsJson)
            .MaximumLength(20000)
            .When(x => x.OptionsJson is not null);
    }
}
