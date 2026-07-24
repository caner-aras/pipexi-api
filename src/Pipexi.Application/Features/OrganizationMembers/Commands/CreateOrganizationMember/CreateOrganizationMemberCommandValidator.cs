using FluentValidation;

namespace Pipexi.Application.Features.OrganizationMembers.Commands.CreateOrganizationMember;

public sealed class CreateOrganizationMemberCommandValidator : AbstractValidator<CreateOrganizationMemberCommand>
{
    public CreateOrganizationMemberCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.JobTitle)
            .MaximumLength(100)
            .When(x => x.JobTitle is not null);
    }
}
