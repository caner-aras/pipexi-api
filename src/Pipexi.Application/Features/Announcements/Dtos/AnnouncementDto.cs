namespace Workforce.Application.Features.Announcements.Dtos;

public sealed record AnnouncementDto(
    Guid Id,
    Guid OrganizationId,
    string Title,
    string Body,
    string AudienceType,
    Guid? AudienceId,
    DateTimeOffset? PublishedAt,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
