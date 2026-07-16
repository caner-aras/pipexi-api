namespace Workforce.Domain.Entities;

public sealed class LocationWorkingHour : BaseEntity
{
    private LocationWorkingHour(
        Guid id,
        Guid locationId,
        int dayOfWeek,
        bool isClosed,
        TimeOnly? opensAt,
        TimeOnly? closesAt,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        LocationId = locationId;
        DayOfWeek = dayOfWeek;
        IsClosed = isClosed;
        OpensAt = opensAt;
        ClosesAt = closesAt;
        UpdatedAt = updatedAt;
    }

    public Guid LocationId { get; private set; }
    public int DayOfWeek { get; private set; }
    public bool IsClosed { get; private set; }
    public TimeOnly? OpensAt { get; private set; }
    public TimeOnly? ClosesAt { get; private set; }

    public static LocationWorkingHour Create(
        Guid locationId,
        int dayOfWeek,
        bool isClosed,
        TimeOnly? opensAt,
        TimeOnly? closesAt)
    {
        Validate(dayOfWeek, isClosed, opensAt, closesAt);

        return new LocationWorkingHour(
            Guid.NewGuid(),
            locationId,
            dayOfWeek,
            isClosed,
            isClosed ? null : opensAt,
            isClosed ? null : closesAt,
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(bool isClosed, TimeOnly? opensAt, TimeOnly? closesAt, string? status)
    {
        Validate(DayOfWeek, isClosed, opensAt, closesAt);

        IsClosed = isClosed;
        OpensAt = isClosed ? null : opensAt;
        ClosesAt = isClosed ? null : closesAt;

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
            return;
        }

        Touch();
    }

    private static void Validate(int dayOfWeek, bool isClosed, TimeOnly? opensAt, TimeOnly? closesAt)
    {
        if (dayOfWeek < 0 || dayOfWeek > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(dayOfWeek), "DayOfWeek must be between 0 and 6.");
        }

        if (isClosed)
        {
            return;
        }

        if (!opensAt.HasValue || !closesAt.HasValue)
        {
            throw new ArgumentException("Open and close times are required when day is not closed.");
        }

        if (opensAt.Value >= closesAt.Value)
        {
            throw new ArgumentException("Close time must be after open time.");
        }
    }
}