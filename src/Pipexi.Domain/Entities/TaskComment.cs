namespace Workforce.Domain.Entities;

public sealed class TaskComment : BaseEntity
{
    private TaskComment(
        Guid id,
        Guid workTaskId,
        Guid teamMemberId,
        string message,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        WorkTaskId = workTaskId;
        TeamMemberId = teamMemberId;
        Message = message;
        UpdatedAt = updatedAt;
    }

    public Guid WorkTaskId { get; private set; }
    public Guid TeamMemberId { get; private set; }
    public string Message { get; private set; }

    public static TaskComment Create(Guid workTaskId, Guid teamMemberId, string message)
    {
        return new TaskComment(
            Guid.NewGuid(),
            workTaskId,
            teamMemberId,
            message.Trim(),
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(string? message, string? status)
    {
        if (message is not null)
        {
            Message = message.Trim();
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (message is not null || status is not null)
        {
            Touch();
        }
    }
}
