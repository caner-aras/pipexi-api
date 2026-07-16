namespace Workforce.Contracts.V1.LeaveRequests;

public sealed record CreateLeaveRequestRequest(
    Guid OrganizationId,
    Guid OrganizationMemberId,
    string LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    string Reason);
