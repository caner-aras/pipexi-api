namespace Pipexi.Domain.Entities;

public sealed class ShiftBreak : BaseEntity
{
    private ShiftBreak(
        Guid id,
        Guid shiftId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        bool isPaid,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        ShiftId = shiftId;
        StartAt = startAt;
        EndAt = endAt;
        IsPaid = isPaid;
        UpdatedAt = updatedAt;
    }

    public Guid ShiftId { get; private set; }
    public DateTimeOffset StartAt { get; private set; }
    public DateTimeOffset EndAt { get; private set; }
    public bool IsPaid { get; private set; }

    public static ShiftBreak Create(
        Guid shiftId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        bool isPaid)
    {
        return new ShiftBreak(
            Guid.NewGuid(),
            shiftId,
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
