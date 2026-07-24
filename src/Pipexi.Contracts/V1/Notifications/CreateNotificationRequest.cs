namespace Pipexi.Contracts.V1.Notifications;

public sealed record CreateNotificationRequest(
    Guid OrganizationId,
    Guid OrganizationMemberId,
    string Type,
    string Title,
    string Body,
    bool IsRead = false,
    DateTimeOffset? ScheduledTime = null);
