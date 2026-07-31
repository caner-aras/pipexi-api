using Pipexi.Application.Features.Conversations.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.Conversations;

public static class ConversationMappings
{
    public static ConversationMessageDto ToDto(this ConversationMessage entity)
    {
        return new ConversationMessageDto(
            entity.Id,
            entity.ConversationId,
            entity.SenderOrganizationMemberId,
            entity.Body,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
