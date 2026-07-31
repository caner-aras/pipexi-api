namespace Pipexi.Domain.Entities;

public sealed class ConversationMember : BaseEntity
{
    private ConversationMember(
        Guid id,
        Guid conversationId,
        Guid organizationMemberId,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? lastReadAt = null,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        ConversationId = conversationId;
        OrganizationMemberId = organizationMemberId;
        LastReadAt = lastReadAt;
        UpdatedAt = updatedAt;
    }

    public Guid ConversationId { get; private set; }
    public Guid OrganizationMemberId { get; private set; }
    public DateTimeOffset? LastReadAt { get; private set; }

    public static ConversationMember Create(Guid conversationId, Guid organizationMemberId)
    {
        var now = DateTimeOffset.UtcNow;
        return new ConversationMember(
            Guid.NewGuid(),
            conversationId,
            organizationMemberId,
            "active",
            now,
            lastReadAt: now);
    }

    public void MarkRead(DateTimeOffset at)
    {
        if (LastReadAt.HasValue && LastReadAt.Value >= at)
        {
            return;
        }

        LastReadAt = at;
        Touch();
    }
}
