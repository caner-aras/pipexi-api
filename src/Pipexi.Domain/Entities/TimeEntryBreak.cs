namespace Workforce.Domain.Entities;

public sealed class TimeEntryBreak : BaseEntity
{
    private TimeEntryBreak(
        Guid id,
        Guid timeEntryId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        bool isPaid,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        TimeEntryId = timeEntryId;
        StartAt = startAt;
        EndAt = endAt;
        IsPaid = isPaid;
        UpdatedAt = updatedAt;
    }

    public Guid TimeEntryId { get; private set; }
    public DateTimeOffset StartAt { get; private set; }
    public DateTimeOffset EndAt { get; private set; }
    public bool IsPaid { get; private set; }

    public static TimeEntryBreak Create(
        Guid timeEntryId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        bool isPaid)
    {
        return new TimeEntryBreak(
            Guid.NewGuid(),
            timeEntryId,
            startAt,
            endAt,
            isPaid,
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(
        DateTimeOffset? startAt,
        DateTimeOffset? endAt,
        bool? isPaid,
        string? status)
    {
        if (startAt.HasValue)
        {
            StartAt = startAt.Value;
        }

        if (endAt.HasValue)
        {
            EndAt = endAt.Value;
        }

        if (isPaid.HasValue)
        {
            IsPaid = isPaid.Value;
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (startAt.HasValue || endAt.HasValue || isPaid.HasValue || status is not null)
        {
            Touch();
        }
    }
}
