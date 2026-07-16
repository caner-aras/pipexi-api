namespace Workforce.Contracts.V1.LeaveRequests;

public sealed record UpdateLeaveRequestRequest(
    string? LeaveType,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Status,
    string? Reason);
