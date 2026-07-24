using FluentValidation;

namespace Pipexi.Application.Features.Forms.Commands.UpdateFormTemplate;

public sealed class UpdateFormTemplateCommandValidator : AbstractValidator<UpdateFormTemplateCommand>
{
    public UpdateFormTemplateCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .MaximumLength(200)
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null);

        RuleFor(x => x.Status)
            .MaximumLength(30)
            .When(x => x.Status is not null);
    }
}
