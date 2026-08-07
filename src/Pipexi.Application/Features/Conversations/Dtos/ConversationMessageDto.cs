namespace Pipexi.Application.Features.Conversations.Dtos;

public sealed record ConversationMessageReactionDto(
    string Emoji,
    int Count,
    bool ReactedByMe);

public sealed record ConversationMessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderOrganizationMemberId,
    string SenderDisplayName,
    bool IsMine,
    bool IsDeleted,
    bool CanDelete,
    bool CanEdit,
    bool IsEdited,
    string Body,
    IReadOnlyCollection<ConversationMessageReactionDto> Reactions,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? EditedAt);
