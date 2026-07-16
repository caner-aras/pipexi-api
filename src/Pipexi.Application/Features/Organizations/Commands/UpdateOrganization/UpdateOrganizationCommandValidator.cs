using FluentValidation;
using Workforce.Domain.Time;

namespace Workforce.Application.Features.Organizations.Commands.UpdateOrganization;

public sealed class UpdateOrganizationCommandValidator : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .MaximumLength(200)
            .When(x => x.Name is not null);

        RuleFor(x => x.Name)
            .NotEmpty()
            .When(x => x.Name is not null);

        RuleFor(x => x.Slug)
            .MaximumLength(100)
            .When(x => x.Slug is not null);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .When(x => x.Slug is not null);

        RuleFor(x => x.Timezone)
            .MaximumLength(100)
            .Must(IanaTimeZone.IsValid)
            .WithMessage("Timezone must be a valid IANA time zone ID, e.g. Europe/Istanbul.")
            .When(x => x.Timezone is not null);

        RuleFor(x => x.Timezone)
            .NotEmpty()
            .When(x => x.Timezone is not null);

        RuleFor(x => x.Status)
            .MaximumLength(50)
            .When(x => x.Status is not null);

        RuleFor(x => x.Status)
            .NotEmpty()
            .When(x => x.Status is not null);
    }
}
