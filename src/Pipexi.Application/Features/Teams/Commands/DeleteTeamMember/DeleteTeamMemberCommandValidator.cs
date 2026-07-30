using FluentValidation;

namespace Pipexi.Application.Features.Teams.Commands.DeleteTeamMember;

public sealed class DeleteTeamMemberCommandValidator : AbstractValidator<DeleteTeamMemberCommand>
{
    public DeleteTeamMemberCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
