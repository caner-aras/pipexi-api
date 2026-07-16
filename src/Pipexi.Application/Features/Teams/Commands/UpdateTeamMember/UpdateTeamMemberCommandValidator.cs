using FluentValidation;

namespace Workforce.Application.Features.Teams.Commands.UpdateTeamMember;

public sealed class UpdateTeamMemberCommandValidator : AbstractValidator<UpdateTeamMemberCommand>
{
    public UpdateTeamMemberCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Status)
            .NotEmpty()
            .MaximumLength(50)
            .When(x => x.Status is not null);
    }
}
