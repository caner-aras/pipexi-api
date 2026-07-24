using FluentValidation;

namespace Pipexi.Application.Features.OrganizationMembers.Commands.UpdateOrganizationMember;

public sealed class UpdateOrganizationMemberCommandValidator : AbstractValidator<UpdateOrganizationMemberCommand>
{
    public UpdateOrganizationMemberCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.RoleId)
            .NotEmpty()
            .When(x => x.RoleId.HasValue);

        RuleFor(x => x.JobTitle)
            .MaximumLength(100)
            .When(x => x.JobTitle is not null);

        RuleFor(x => x.Status)
            .MaximumLength(50)
            .When(x => x.Status is not null);

        RuleFor(x => x.Status)
            .NotEmpty()
            .When(x => x.Status is not null);
    }
}
