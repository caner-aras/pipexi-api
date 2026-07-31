namespace Pipexi.Domain.Entities;

public sealed class Conversation : BaseEntity
{
    public const string TypeDirect = "direct";
    public const string TypeGroup = "group";

    private Conversation(
        Guid id,
        Guid organizationId,
        string type,
        string? title,
        string? directMemberPairKey,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationId = organizationId;
        Type = type;
        Title = title;
        DirectMemberPairKey = directMemberPairKey;
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationId { get; private set; }
    public string Type { get; private set; }
    public string? Title { get; private set; }
    public string? DirectMemberPairKey { get; private set; }

    public void MarkActivity()
    {
        Touch();
    }

    public static string BuildDirectMemberPairKey(Guid memberA, Guid memberB)
    {
        var first = memberA.CompareTo(memberB) <= 0 ? memberA : memberB;
        var second = memberA.CompareTo(memberB) <= 0 ? memberB : memberA;
        return $"{first:D}:{second:D}";
    }

    public static Conversation CreateDirect(Guid organizationId, Guid memberA, Guid memberB)
    {
        if (memberA == memberB)
        {
            throw new ArgumentException("Direct conversation requires two distinct members.");
        }

        return new Conversation(
            Guid.NewGuid(),
            organizationId,
            TypeDirect,
            title: null,
            BuildDirectMemberPairKey(memberA, memberB),
            "active",
            DateTimeOffset.UtcNow);
    }

    public static Conversation CreateGroup(Guid organizationId, string title)
    {
        var normalizedTitle = title.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            throw new ArgumentException("Group title is required.");
        }

        return new Conversation(
            Guid.NewGuid(),
            organizationId,
            TypeGroup,
            normalizedTitle,
            directMemberPairKey: null,
            "active",
            DateTimeOffset.UtcNow);
    }
}
