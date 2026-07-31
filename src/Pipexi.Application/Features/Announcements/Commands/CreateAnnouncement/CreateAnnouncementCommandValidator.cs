using FluentValidation;

namespace Pipexi.Application.Features.Announcements.Commands.CreateAnnouncement;

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
            .MaximumLength(50)
            .Must(type => AnnouncementAudience.AllowedTypes.Contains(type.Trim()))
            .WithMessage("AudienceType must be all, location, role, member, or team.");

        RuleFor(x => x.AudienceId)
            .Null()
            .When(x => AnnouncementAudience.IsAll(x.AudienceType))
            .WithMessage("AudienceId must be empty when AudienceType is all.");

        RuleFor(x => x.AudienceId)
            .NotEmpty()
            .When(x => AnnouncementAudience.RequiresAudienceId(x.AudienceType))
            .WithMessage("AudienceId is required for the selected AudienceType.");
    }
}
