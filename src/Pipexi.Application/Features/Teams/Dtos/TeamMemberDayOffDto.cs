namespace Workforce.Application.Features.Teams.Dtos;

public sealed record TeamMemberDayOffDto(
    Guid Id,
    Guid TeamMemberId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string? Reason,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);