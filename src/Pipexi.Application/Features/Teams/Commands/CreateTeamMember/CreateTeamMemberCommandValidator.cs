using FluentValidation;

namespace Workforce.Application.Features.Teams.Commands.CreateTeamMember;

public sealed class CreateTeamMemberCommandValidator : AbstractValidator<CreateTeamMemberCommand>
{
    public CreateTeamMemberCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.OrganizationMemberId).NotEmpty();
    }
}
