namespace Pipexi.Contracts.V1.Announcements;

public sealed record UpdateAnnouncementRequest(
    string? Title,
    string? Body,
    string? AudienceType,
    Guid? AudienceId,
    DateTimeOffset? PublishedAt,
    string? Status);
