using FluentValidation;

namespace Pipexi.Application.Features.Notifications.Commands.UpdateNotification;

public sealed class UpdateNotificationCommandValidator : AbstractValidator<UpdateNotificationCommand>
{
    public UpdateNotificationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Type)
            .MaximumLength(50)
            .When(x => x.Type is not null);

        RuleFor(x => x.Title)
            .MaximumLength(300)
            .When(x => x.Title is not null);

        RuleFor(x => x.Body)
            .MaximumLength(8000)
            .When(x => x.Body is not null);

        RuleFor(x => x.Status)
            .MaximumLength(30)
            .When(x => x.Status is not null);
    }
}
