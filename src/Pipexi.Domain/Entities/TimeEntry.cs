namespace Pipexi.Domain.Entities;

public sealed class TimeEntry : BaseEntity
{
    private TimeEntry(
        Guid id,
        Guid organizationId,
        Guid shiftId,
        Guid organizationMemberId,
        Guid locationId,
        DateTimeOffset clockInAt,
        DateTimeOffset? clockOutAt,
        string? employeeNote,
        string? managerNote,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationId = organizationId;
        ShiftId = shiftId;
        OrganizationMemberId = organizationMemberId;
        LocationId = locationId;
        ClockInAt = clockInAt;
        ClockOutAt = clockOutAt;
        EmployeeNote = employeeNote;
        ManagerNote = managerNote;
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationId { get; private set; }
    public Guid ShiftId { get; private set; }
    public Guid OrganizationMemberId { get; private set; }
    public Guid LocationId { get; private set; }

    public DateTimeOffset ClockInAt { get; private set; }
    public DateTimeOffset? ClockOutAt { get; private set; }

    public string? EmployeeNote { get; private set; }
    public string? ManagerNote { get; private set; }

    public static TimeEntry Create(
        Guid organizationId,
        Guid shiftId,
        Guid organizationMemberId,
        Guid locationId,
        DateTimeOffset clockInAt,
        DateTimeOffset? clockOutAt,
        string? employeeNote,
        string? managerNote)
    {
        return new TimeEntry(
            Guid.NewGuid(),
            organizationId,
            shiftId,
            organizationMemberId,
            locationId,
            clockInAt,
            clockOutAt,
            string.IsNullOrWhiteSpace(employeeNote) ? null : employeeNote.Trim(),
            string.IsNullOrWhiteSpace(managerNote) ? null : managerNote.Trim(),
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(
        Guid? shiftId,
        Guid? organizationMemberId,
        Guid? locationId,
        DateTimeOffset? clockInAt,
        DateTimeOffset? clockOutAt,
        string? employeeNote,
        string? managerNote,
        string? status)
    {
        if (shiftId.HasValue)
        {
            ShiftId = shiftId.Value;
        }

        if (organizationMemberId.HasValue)
        {
            OrganizationMemberId = organizationMemberId.Value;
        }

        if (locationId.HasValue)
        {
            LocationId = locationId.Value;
        }

        if (clockInAt.HasValue)
        {
            ClockInAt = clockInAt.Value;
        }

        if (clockOutAt.HasValue)
        {
            ClockOutAt = clockOutAt.Value;
        }

        if (employeeNote is not null)
        {
            EmployeeNote = string.IsNullOrWhiteSpace(employeeNote) ? null : employeeNote.Trim();
        }

        if (managerNote is not null)
        {
            ManagerNote = string.IsNullOrWhiteSpace(managerNote) ? null : managerNote.Trim();
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (shiftId.HasValue ||
            organizationMemberId.HasValue ||
            locationId.HasValue ||
            clockInAt.HasValue ||
            clockOutAt.HasValue ||
            employeeNote is not null ||
            managerNote is not null ||
            status is not null)
        {
            Touch();
        }
    }
}
