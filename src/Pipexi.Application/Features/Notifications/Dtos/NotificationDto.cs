namespace Pipexi.Application.Features.Notifications.Dtos;

public sealed record NotificationDto(
    Guid Id,
    Guid OrganizationId,
    Guid OrganizationMemberId,
    string Type,
    string Title,
    string Body,
    bool IsRead,
    DateTimeOffset? ScheduledTime,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
