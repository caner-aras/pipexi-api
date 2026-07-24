using FluentValidation;

namespace Pipexi.Application.Features.Teams.Commands.CreateTeamMember;

public sealed class CreateTeamMemberCommandValidator : AbstractValidator<CreateTeamMemberCommand>
{
    public CreateTeamMemberCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.OrganizationMemberId).NotEmpty();
    }
}
