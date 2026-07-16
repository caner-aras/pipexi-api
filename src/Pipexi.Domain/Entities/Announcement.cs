namespace Workforce.Domain.Entities;

public sealed class Announcement : BaseEntity
{
    private Announcement(
        Guid id,
        Guid organizationId,
        string title,
        string body,
        string audienceType,
        Guid? audienceId,
        DateTimeOffset? publishedAt,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationId = organizationId;
        Title = title;
        Body = body;
        AudienceType = audienceType;
        AudienceId = audienceId;
        PublishedAt = publishedAt;
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationId { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public string AudienceType { get; private set; }
    public Guid? AudienceId { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }

    public static Announcement Create(
        Guid organizationId,
        string title,
        string body,
        string audienceType,
        Guid? audienceId,
        DateTimeOffset? publishedAt)
    {
        return new Announcement(
            Guid.NewGuid(),
            organizationId,
            title.Trim(),
            body.Trim(),
            audienceType.Trim().ToLowerInvariant(),
            audienceId,
            publishedAt,
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(
        string? title,
        string? body,
        string? audienceType,
        Guid? audienceId,
        DateTimeOffset? publishedAt,
        string? status)
    {
        if (title is not null)
        {
            Title = title.Trim();
        }

        if (body is not null)
        {
            Body = body.Trim();
        }

        if (audienceType is not null)
        {
            AudienceType = audienceType.Trim().ToLowerInvariant();
        }

        if (audienceId.HasValue)
        {
            AudienceId = audienceId.Value;
        }

        if (publishedAt.HasValue)
        {
            PublishedAt = publishedAt.Value;
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (title is not null ||
            body is not null ||
            audienceType is not null ||
            audienceId.HasValue ||
            publishedAt.HasValue)
        {
            Touch();
        }
    }
}