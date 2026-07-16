using FluentValidation;

namespace Workforce.Application.Features.Forms.Commands.CreateFormAnswer;

public sealed class CreateFormAnswerCommandValidator : AbstractValidator<CreateFormAnswerCommand>
{
    public CreateFormAnswerCommandValidator()
    {
        RuleFor(x => x.FormSubmissionId).NotEmpty();
        RuleFor(x => x.FormFieldId).NotEmpty();

        RuleFor(x => x.Value)
            .MaximumLength(10000)
            .When(x => x.Value is not null);
    }
}
