namespace Pipexi.Domain.Events.Conversations;

public sealed record ConversationMessageCreatedEvent(
    Guid ConversationId,
    Guid MessageId,
    Guid SenderOrganizationMemberId,
    string SenderName,
    string Body,
    DateTimeOffset MessageCreatedAt) : IDomainEvent;
