using FluentValidation;

namespace Workforce.Application.Features.Forms.Commands.UpdateFormAnswer;

public sealed class UpdateFormAnswerCommandValidator : AbstractValidator<UpdateFormAnswerCommand>
{
    public UpdateFormAnswerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Value)
            .MaximumLength(10000)
            .When(x => x.Value is not null);

        RuleFor(x => x.Status)
            .MaximumLength(30)
            .When(x => x.Status is not null);
    }
}
