namespace Pipexi.Domain.Entities;

public sealed class LeaveRequest : BaseEntity
{
    private LeaveRequest(
        Guid id,
        Guid organizationId,
        Guid organizationMemberId,
        string leaveType,
        DateOnly startDate,
        DateOnly endDate,
        string reason,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationId = organizationId;
        OrganizationMemberId = organizationMemberId;
        LeaveType = leaveType;
        StartDate = startDate;
        EndDate = endDate;
        Reason = reason;
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationId { get; private set; }
    public Guid OrganizationMemberId { get; private set; }
    public string LeaveType { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string Reason { get; private set; }

    public static LeaveRequest Create(
        Guid organizationId,
        Guid organizationMemberId,
        string leaveType,
        DateOnly startDate,
        DateOnly endDate,
        string reason)
    {
        return new LeaveRequest(
            Guid.NewGuid(),
            organizationId,
            organizationMemberId,
            leaveType.Trim().ToLowerInvariant(),
            startDate,
            endDate,
            reason.Trim(),
            "pending",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(
        string? leaveType,
        DateOnly? startDate,
        DateOnly? endDate,
        string? reason,
        string? status)
    {
        if (leaveType is not null)
        {
            LeaveType = leaveType.Trim().ToLowerInvariant();
        }

        if (startDate.HasValue)
        {
            StartDate = startDate.Value;
        }

        if (endDate.HasValue)
        {
            EndDate = endDate.Value;
        }

        if (reason is not null)
        {
            Reason = reason.Trim();
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (leaveType is not null || startDate.HasValue || endDate.HasValue || reason is not null)
        {
            Touch();
        }
    }
}
