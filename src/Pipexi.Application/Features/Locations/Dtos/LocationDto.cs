namespace Workforce.Application.Features.Locations.Dtos;

public sealed record LocationDto(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string? Address,
    decimal? Latitude,
    decimal? Longitude,
    int GeofenceRadiusMeters,
    string? Timezone,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyCollection<LocationWorkingHourDto> WorkingHours);
