using FluentValidation;

namespace Pipexi.Application.Features.Forms.Commands.CreateFormTemplate;

public sealed class CreateFormTemplateCommandValidator : AbstractValidator<CreateFormTemplateCommand>
{
    public CreateFormTemplateCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null);
    }
}
