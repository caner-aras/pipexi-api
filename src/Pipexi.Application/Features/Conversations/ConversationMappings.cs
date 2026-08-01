using Pipexi.Application.Features.Conversations.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.Conversations;

public static class ConversationMappings
{
    public static ConversationMessageDto ToDto(
        this ConversationMessage entity,
        string senderDisplayName,
        bool isMine)
    {
        return new ConversationMessageDto(
            entity.Id,
            entity.ConversationId,
            entity.SenderOrganizationMemberId,
            senderDisplayName,
            isMine,
            entity.Body,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    public static string BuildMemberDisplayName(User? user)
    {
        if (user is null)
        {
            return "Member";
        }

        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.Email : fullName;
    }
}
