using FluentValidation;

namespace Workforce.Application.Features.Announcements.Commands.UpdateAnnouncement;

public sealed class UpdateAnnouncementCommandValidator : AbstractValidator<UpdateAnnouncementCommand>
{
    public UpdateAnnouncementCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Title)
            .MaximumLength(300)
            .When(x => x.Title is not null);

        RuleFor(x => x.Body)
            .MaximumLength(8000)
            .When(x => x.Body is not null);

        RuleFor(x => x.AudienceType)
            .MaximumLength(50)
            .When(x => x.AudienceType is not null);

        RuleFor(x => x.Status)
            .MaximumLength(30)
            .When(x => x.Status is not null);
    }
}
