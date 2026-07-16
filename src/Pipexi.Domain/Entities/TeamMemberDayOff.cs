namespace Workforce.Domain.Entities;

public sealed class TeamMemberDayOff : BaseEntity
{
    private TeamMemberDayOff(
        Guid id,
        Guid teamMemberId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        string? reason,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        TeamMemberId = teamMemberId;
        StartAt = startAt;
        EndAt = endAt;
        Reason = reason;
        UpdatedAt = updatedAt;
    }

    public Guid TeamMemberId { get; private set; }
    public DateTimeOffset StartAt { get; private set; }
    public DateTimeOffset EndAt { get; private set; }
    public string? Reason { get; private set; }

    public static TeamMemberDayOff Create(
        Guid teamMemberId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        string? reason)
    {
        return new TeamMemberDayOff(
            Guid.NewGuid(),
            teamMemberId,
            startAt,
            endAt,
            string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(
        DateTimeOffset? startAt,
        DateTimeOffset? endAt,
        string? reason,
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

        if (reason is not null)
        {
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (startAt.HasValue || endAt.HasValue || reason is not null || status is not null)
        {
            Touch();
        }
    }
}