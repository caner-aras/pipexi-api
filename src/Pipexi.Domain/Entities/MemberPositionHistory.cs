namespace Pipexi.Domain.Entities;

public sealed class MemberPositionHistory : BaseEntity
{
    private MemberPositionHistory(
        Guid id,
        Guid organizationMemberId,
        Guid positionId,
        decimal hourlyRate,
        DateTimeOffset startDate,
        DateTimeOffset? endDate,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationMemberId = organizationMemberId;
        PositionId = positionId;
        HourlyRate = hourlyRate;
        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationMemberId { get; private set; }
    public Guid PositionId { get; private set; }
    public decimal HourlyRate { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset? EndDate { get; private set; }

    public static MemberPositionHistory Create(
        Guid organizationMemberId,
        Guid positionId,
        decimal hourlyRate,
        DateTimeOffset? startDate = null)
    {
        return new MemberPositionHistory(
            Guid.NewGuid(),
            organizationMemberId,
            positionId,
            hourlyRate,
            startDate ?? DateTimeOffset.UtcNow,
            null,
            "active",
            DateTimeOffset.UtcNow);
    }

    public void EndAssignment(DateTimeOffset? endDate = null)
    {
        EndDate = endDate ?? DateTimeOffset.UtcNow;
        SetStatus("ended");
        Touch();
    }

    public void UpdateDetails(decimal? hourlyRate, DateTimeOffset? startDate, DateTimeOffset? endDate, string? status)
    {
        if (hourlyRate.HasValue)
        {
            HourlyRate = hourlyRate.Value;
        }

        if (startDate.HasValue)
        {
            StartDate = startDate.Value;
        }

        if (endDate.HasValue)
        {
            EndDate = endDate.Value;
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (hourlyRate.HasValue || startDate.HasValue || endDate.HasValue || status is not null)
        {
            Touch();
        }
    }
}
