using FluentValidation;

namespace Workforce.Application.Features.Announcements.Commands.CreateAnnouncement;

public sealed class CreateAnnouncementCommandValidator : AbstractValidator<CreateAnnouncementCommand>
{
    public CreateAnnouncementCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.Body)
            .NotEmpty()
            .MaximumLength(8000);

        RuleFor(x => x.AudienceType)
            .NotEmpty()
            .MaximumLength(50);
    }
}
