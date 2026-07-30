using FluentValidation;

namespace Pipexi.Application.Features.Teams.Commands.DeleteTeam;

public sealed class DeleteTeamCommandValidator : AbstractValidator<DeleteTeamCommand>
{
    public DeleteTeamCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
