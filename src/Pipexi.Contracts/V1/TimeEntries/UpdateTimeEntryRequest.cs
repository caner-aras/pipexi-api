namespace Workforce.Contracts.V1.TimeEntries;

public sealed record UpdateTimeEntryRequest(
    Guid? ShiftId,
    Guid? OrganizationMemberId,
    Guid? LocationId,
    DateTimeOffset? ClockInAt,
    DateTimeOffset? ClockOutAt,
    string? EmployeeNote,
    string? ManagerNote,
    string? Status);
