namespace Pipexi.Domain.Entities;

public sealed class Notification : BaseEntity
{
    private Notification(
        Guid id,
        Guid organizationId,
        Guid organizationMemberId,
        string type,
        string title,
        string body,
        bool isRead,
        DateTimeOffset? scheduledTime,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationId = organizationId;
        OrganizationMemberId = organizationMemberId;
        Type = type;
        Title = title;
        Body = body;
        IsRead = isRead;
        ScheduledTime = scheduledTime;
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationId { get; private set; }
    public Guid OrganizationMemberId { get; private set; }
    public string Type { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public bool IsRead { get; private set; }
    public DateTimeOffset? ScheduledTime { get; private set; }

    public static Notification Create(
        Guid organizationId,
        Guid organizationMemberId,
        string type,
        string title,
        string body,
        bool isRead,
        DateTimeOffset? scheduledTime)
    {
        return new Notification(
            Guid.NewGuid(),
            organizationId,
            organizationMemberId,
            type.Trim().ToLowerInvariant(),
            title.Trim(),
            body.Trim(),
            isRead,
            scheduledTime,
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(string? type, string? title, string? body, bool? isRead, DateTimeOffset? scheduledTime, string? status)
    {
        if (type is not null)
        {
            Type = type.Trim().ToLowerInvariant();
        }

        if (title is not null)
        {
            Title = title.Trim();
        }

        if (body is not null)
        {
            Body = body.Trim();
        }

        if (isRead.HasValue)
        {
            IsRead = isRead.Value;
        }

        ScheduledTime = scheduledTime;

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        Touch();
    }
}
