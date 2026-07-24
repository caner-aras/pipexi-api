namespace Pipexi.Contracts.V1.Notifications;

public sealed record UpdateNotificationRequest(
    string? Type,
    string? Title,
    string? Body,
    bool? IsRead,
    DateTimeOffset? ScheduledTime,
    string? Status);
