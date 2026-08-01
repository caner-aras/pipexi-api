namespace Pipexi.Application.Features.Conversations.Dtos;

public sealed record ConversationMessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderOrganizationMemberId,
    string SenderDisplayName,
    bool IsMine,
    string Body,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
