using FluentValidation;

namespace Workforce.Application.Features.Teams.Commands.CreateTeam;

public sealed class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ManagerMemberId)
            .NotEmpty()
            .When(x => x.ManagerMemberId.HasValue);
    }
}
