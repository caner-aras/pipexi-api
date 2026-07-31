using FluentValidation;

namespace Pipexi.Application.Features.Conversations.Commands.CreateConversationMessage;

public sealed class CreateConversationMessageCommandValidator
    : AbstractValidator<CreateConversationMessageCommand>
{
    public CreateConversationMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(8000);
    }
}
