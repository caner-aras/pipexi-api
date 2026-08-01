namespace Pipexi.Application.Features.Teams.Dtos;

public sealed record ActiveDayOffDto(
    Guid Id,
    Guid TeamMemberId,
    string MemberName,
    string? AvatarUrl,
    string TeamName,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string? Reason);
