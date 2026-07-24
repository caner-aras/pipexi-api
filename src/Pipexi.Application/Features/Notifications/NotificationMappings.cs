using Pipexi.Application.Features.Notifications.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.Notifications;

internal static class NotificationMappings
{
    public static NotificationDto ToDto(this Notification notification)
    {
        return new NotificationDto(
            notification.Id,
            notification.OrganizationId,
            notification.OrganizationMemberId,
            notification.Type,
            notification.Title,
            notification.Body,
            notification.IsRead,
            notification.ScheduledTime,
            notification.Status,
            notification.CreatedAt,
            notification.UpdatedAt);
    }
}
