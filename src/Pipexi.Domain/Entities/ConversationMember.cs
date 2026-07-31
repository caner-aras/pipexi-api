namespace Pipexi.Domain.Entities;

public sealed class ConversationMember : BaseEntity
{
    private ConversationMember(
        Guid id,
        Guid conversationId,
        Guid organizationMemberId,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        ConversationId = conversationId;
        OrganizationMemberId = organizationMemberId;
        UpdatedAt = updatedAt;
    }

    public Guid ConversationId { get; private set; }
    public Guid OrganizationMemberId { get; private set; }

    public static ConversationMember Create(Guid conversationId, Guid organizationMemberId)
    {
        return new ConversationMember(
            Guid.NewGuid(),
            conversationId,
            organizationMemberId,
            "active",
            DateTimeOffset.UtcNow);
    }
}
