namespace Pipexi.Application.Features.Organizations.Dtos;

public sealed record OrganizationDto(
    Guid Id,
    string Name,
    string Slug,
    string Timezone,
    string Currency,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
