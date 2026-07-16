using Workforce.Application.Features.Announcements.Dtos;
using Workforce.Domain.Entities;

namespace Workforce.Application.Features.Announcements;

internal static class AnnouncementMappings
{
    public static AnnouncementDto ToDto(this Announcement announcement)
    {
        return new AnnouncementDto(
            announcement.Id,
            announcement.OrganizationId,
            announcement.Title,
            announcement.Body,
            announcement.AudienceType,
            announcement.AudienceId,
            announcement.PublishedAt,
            announcement.Status,
            announcement.CreatedAt,
            announcement.UpdatedAt);
    }
}
