namespace Workforce.Application.Features.Organizations.Dtos;

public sealed record OrganizationDto(
    Guid Id,
    string Name,
    string Slug,
    string Timezone,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
