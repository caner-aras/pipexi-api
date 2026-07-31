namespace Pipexi.Domain.Entities;

public sealed class ConversationMessage : BaseEntity
{
    private ConversationMessage(
        Guid id,
        Guid conversationId,
        Guid senderOrganizationMemberId,
        string body,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        ConversationId = conversationId;
        SenderOrganizationMemberId = senderOrganizationMemberId;
        Body = body;
        UpdatedAt = updatedAt;
    }

    public Guid ConversationId { get; private set; }
    public Guid SenderOrganizationMemberId { get; private set; }
    public string Body { get; private set; }

    public static ConversationMessage Create(
        Guid conversationId,
        Guid senderOrganizationMemberId,
        string body)
    {
        return new ConversationMessage(
            Guid.NewGuid(),
            conversationId,
            senderOrganizationMemberId,
            body.Trim(),
            "active",
            DateTimeOffset.UtcNow);
    }
}
