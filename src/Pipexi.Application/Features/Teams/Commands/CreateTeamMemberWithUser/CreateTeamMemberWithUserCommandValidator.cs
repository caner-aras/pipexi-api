using FluentValidation;

namespace Pipexi.Application.Features.Teams.Commands.CreateTeamMemberWithUser;

public sealed class CreateTeamMemberWithUserCommandValidator : AbstractValidator<CreateTeamMemberWithUserCommand>
{
    public CreateTeamMemberWithUserCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.JobTitle)
            .MaximumLength(200)
            .When(x => x.JobTitle is not null);

        RuleFor(x => x.Phone)
            .MaximumLength(50)
            .When(x => x.Phone is not null);

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500)
            .When(x => x.AvatarUrl is not null);

        RuleFor(x => x.AuthProviderId)
            .MaximumLength(200)
            .When(x => x.AuthProviderId is not null);
    }
}
