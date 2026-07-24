namespace Pipexi.Application.Features.MemberPositions.Dtos;

public sealed record MemberPositionHistoryDto(
    Guid Id,
    Guid OrganizationMemberId,
    Guid PositionId,
    decimal HourlyRate,
    DateTimeOffset StartDate,
    DateTimeOffset? EndDate,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
