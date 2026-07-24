using FluentValidation;

namespace Pipexi.Application.Features.Teams.Commands.UpdateTeam;

public sealed class UpdateTeamCommandValidator : AbstractValidator<UpdateTeamCommand>
{
    public UpdateTeamCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150)
            .When(x => x.Name is not null);

        RuleFor(x => x.ManagerMemberId)
            .NotEmpty()
            .When(x => x.ManagerMemberId.HasValue);

        RuleFor(x => x.Status)
            .NotEmpty()
            .MaximumLength(50)
            .When(x => x.Status is not null);
    }
}
