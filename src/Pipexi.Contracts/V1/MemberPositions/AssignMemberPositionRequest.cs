namespace Pipexi.Contracts.V1.MemberPositions;

public sealed record AssignMemberPositionRequest(
    Guid OrganizationMemberId,
    Guid PositionId,
    decimal HourlyRate,
    DateTimeOffset? StartDate);
