using FluentValidation;

namespace Pipexi.Application.Features.Conversations.Commands.CreateConversation;

public sealed class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.PeerOrganizationMemberId).NotEmpty();
    }
}
