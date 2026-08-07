using FluentValidation;

namespace Pipexi.Application.Features.Conversations.Commands.EditConversationMessage;

public sealed class EditConversationMessageCommandValidator
    : AbstractValidator<EditConversationMessageCommand>
{
    public EditConversationMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(8000);
    }
}
