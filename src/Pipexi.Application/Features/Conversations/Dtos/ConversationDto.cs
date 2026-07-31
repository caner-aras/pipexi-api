namespace Pipexi.Application.Features.Conversations.Dtos;

public sealed record ConversationDto(
    Guid Id,
    Guid OrganizationId,
    string Type,
    string? Title,
    Guid? PeerOrganizationMemberId,
    string PeerDisplayName,
    string? PeerAvatarUrl,
    string? LastMessageBody,
    DateTimeOffset? LastMessageAt,
    int UnreadCount,
    int MemberCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
