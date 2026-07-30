using FluentValidation;

namespace Pipexi.Application.Features.OrganizationMembers.Commands.DeleteOrganizationMember;

public sealed class DeleteOrganizationMemberCommandValidator : AbstractValidator<DeleteOrganizationMemberCommand>
{
    public DeleteOrganizationMemberCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
