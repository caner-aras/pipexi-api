using FluentValidation;

namespace Pipexi.Application.Features.Forms.Commands.UpdateFormSubmission;

public sealed class UpdateFormSubmissionCommandValidator : AbstractValidator<UpdateFormSubmissionCommand>
{
    public UpdateFormSubmissionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Status)
            .MaximumLength(30)
            .When(x => x.Status is not null);
    }
}
