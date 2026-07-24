using FluentValidation;
using Pipexi.Domain.Time;

namespace Pipexi.Application.Features.Organizations.Commands.CreateOrganization;

public sealed class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Timezone)
            .NotEmpty()
            .MaximumLength(100)
            .Must(IanaTimeZone.IsValid)
            .WithMessage("Timezone must be a valid IANA time zone ID, e.g. Europe/Istanbul.");
    }
}
