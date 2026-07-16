using FluentValidation;

namespace Workforce.Application.Features.Forms.Commands.CreateFormSubmission;

public sealed class CreateFormSubmissionCommandValidator : AbstractValidator<CreateFormSubmissionCommand>
{
    public CreateFormSubmissionCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.FormTemplateId).NotEmpty();
        RuleFor(x => x.SubmittedByMemberId).NotEmpty();
    }
}
