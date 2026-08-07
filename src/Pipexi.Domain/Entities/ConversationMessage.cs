using System.Text.Json;

namespace Pipexi.Domain.Entities;

public sealed class ConversationMessage : BaseEntity
{
    private static readonly HashSet<string> AllowedEmojis = new(StringComparer.Ordinal)
    {
        "👍", "❤️", "😂", "😮", "😢", "🙏"
    };

    private static readonly JsonSerializerOptions ReactionJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private ConversationMessage(
        Guid id,
        Guid conversationId,
        Guid senderOrganizationMemberId,
        string body,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null,
        string? reactionsJson = null,
        DateTimeOffset? editedAt = null)
        : base(id, status, createdAt)
    {
        ConversationId = conversationId;
        SenderOrganizationMemberId = senderOrganizationMemberId;
        Body = body;
        UpdatedAt = updatedAt;
        ReactionsJson = reactionsJson;
        EditedAt = editedAt;
    }

    public Guid ConversationId { get; private set; }
    public Guid SenderOrganizationMemberId { get; private set; }
    public string Body { get; private set; }
    public string? ReactionsJson { get; private set; }
    public DateTimeOffset? EditedAt { get; private set; }

    public bool IsDeleted => Status == "deleted";
    public bool IsEdited => EditedAt.HasValue;

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

    public void SoftDeleteContent()
    {
        Body = string.Empty;
        ReactionsJson = null;
        EditedAt = null;
        MarkDeleted();
    }

    public bool EditBody(string body)
    {
        if (IsDeleted)
        {
            return false;
        }

        var trimmed = body.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.Length > 8000)
        {
            return false;
        }

        if (string.Equals(Body, trimmed, StringComparison.Ordinal))
        {
            return true;
        }

        Body = trimmed;
        EditedAt = DateTimeOffset.UtcNow;
        Touch();
        return true;
    }

    public IReadOnlyList<ConversationMessageReaction> GetReactions()
    {
        if (string.IsNullOrWhiteSpace(ReactionsJson))
        {
            return Array.Empty<ConversationMessageReaction>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<ConversationMessageReaction>>(
                       ReactionsJson,
                       ReactionJsonOptions)
                   ?? [];
        }
        catch (JsonException)
        {
            return Array.Empty<ConversationMessageReaction>();
        }
    }

    public bool ToggleReaction(Guid organizationMemberId, string emoji)
    {
        if (IsDeleted)
        {
            return false;
        }

        var normalized = emoji.Trim();
        if (!AllowedEmojis.Contains(normalized))
        {
            return false;
        }

        var reactions = GetReactions().ToList();
        var existing = reactions.FirstOrDefault(x => x.OrganizationMemberId == organizationMemberId);

        if (existing is not null && existing.Emoji == normalized)
        {
            reactions.Remove(existing);
        }
        else if (existing is not null)
        {
            reactions.Remove(existing);
            reactions.Add(new ConversationMessageReaction(normalized, organizationMemberId));
        }
        else
        {
            reactions.Add(new ConversationMessageReaction(normalized, organizationMemberId));
        }

        ReactionsJson = reactions.Count == 0
            ? null
            : JsonSerializer.Serialize(reactions, ReactionJsonOptions);
        Touch();
        return true;
    }

    public static bool IsAllowedEmoji(string emoji) => AllowedEmojis.Contains(emoji.Trim());
}

public sealed record ConversationMessageReaction(string Emoji, Guid OrganizationMemberId);
