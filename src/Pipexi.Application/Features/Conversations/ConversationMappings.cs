using Pipexi.Application.Features.Conversations.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.Conversations;

public static class ConversationMappings
{
    public static ConversationMessageDto ToDto(
        this ConversationMessage entity,
        string senderDisplayName,
        bool isMine,
        bool canDelete = false)
    {
        var isDeleted = entity.IsDeleted;
        var reactions = entity.GetReactions()
            .GroupBy(x => x.Emoji)
            .Select(group => new ConversationMessageReactionDto(
                group.Key,
                group.Count(),
                false))
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Emoji)
            .ToList();

        return new ConversationMessageDto(
            entity.Id,
            entity.ConversationId,
            entity.SenderOrganizationMemberId,
            senderDisplayName,
            isMine,
            isDeleted,
            canDelete && isMine && !isDeleted,
            isDeleted ? string.Empty : entity.Body,
            reactions,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    public static ConversationMessageDto ToDto(
        this ConversationMessage entity,
        string senderDisplayName,
        Guid currentOrganizationMemberId,
        bool canDelete)
    {
        var isMine = entity.SenderOrganizationMemberId == currentOrganizationMemberId;
        var isDeleted = entity.IsDeleted;
        var reactions = entity.GetReactions()
            .GroupBy(x => x.Emoji)
            .Select(group => new ConversationMessageReactionDto(
                group.Key,
                group.Count(),
                group.Any(x => x.OrganizationMemberId == currentOrganizationMemberId)))
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Emoji)
            .ToList();

        return new ConversationMessageDto(
            entity.Id,
            entity.ConversationId,
            entity.SenderOrganizationMemberId,
            senderDisplayName,
            isMine,
            isDeleted,
            canDelete && isMine && !isDeleted,
            isDeleted ? string.Empty : entity.Body,
            reactions,
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

    public static bool IsMessageUnreadByPeers(
        DateTimeOffset messageCreatedAt,
        Guid senderOrganizationMemberId,
        IReadOnlyCollection<ConversationMember> members)
    {
        return !members.Any(member =>
            member.OrganizationMemberId != senderOrganizationMemberId
            && member.LastReadAt.HasValue
            && member.LastReadAt.Value >= messageCreatedAt);
    }
}
