using FluentValidation;

namespace Pipexi.Application.Features.Announcements.Commands.UpdateAnnouncement;

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
            .Must(type => type is null || AnnouncementAudience.AllowedTypes.Contains(type.Trim()))
            .WithMessage("AudienceType must be all, location, role, member, or team.")
            .When(x => x.AudienceType is not null);

        RuleFor(x => x.AudienceId)
            .Null()
            .When(x => x.AudienceType is not null && AnnouncementAudience.IsAll(x.AudienceType))
            .WithMessage("AudienceId must be empty when AudienceType is all.");

        RuleFor(x => x.AudienceId)
            .NotEmpty()
            .When(x =>
                x.AudienceType is not null &&
                AnnouncementAudience.RequiresAudienceId(x.AudienceType))
            .WithMessage("AudienceId is required for the selected AudienceType.");

        RuleFor(x => x.Status)
            .MaximumLength(30)
            .When(x => x.Status is not null);
    }
}
