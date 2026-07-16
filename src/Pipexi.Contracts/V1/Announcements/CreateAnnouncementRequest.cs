namespace Workforce.Contracts.V1.Announcements;

public sealed record CreateAnnouncementRequest(
    Guid OrganizationId,
    string Title,
    string Body,
    string AudienceType,
    Guid? AudienceId,
    DateTimeOffset? PublishedAt);
