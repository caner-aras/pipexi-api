using FluentValidation;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.Conversations.Commands.CreateConversation;

public sealed class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => x.Title is not null);

        RuleFor(x => x.PeerOrganizationMemberId)
            .NotEmpty()
            .When(x => string.IsNullOrWhiteSpace(x.Type)
                       || string.Equals(x.Type, Conversation.TypeDirect, StringComparison.OrdinalIgnoreCase));

        RuleFor(x => x.Title)
            .NotEmpty()
            .When(x => string.Equals(x.Type, Conversation.TypeGroup, StringComparison.OrdinalIgnoreCase));

        RuleFor(x => x.OrganizationMemberIds)
            .NotNull()
            .Must(ids => ids is not null && ids.Count >= 2)
            .When(x => string.Equals(x.Type, Conversation.TypeGroup, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Group chat requires at least two other members.");
    }
}
