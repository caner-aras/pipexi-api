namespace Pipexi.Contracts.V1.Locations;

public sealed record CreateLocationRequest(
    Guid OrganizationId,
    string Name,
    string? Address,
    decimal? Latitude,
    decimal? Longitude,
    int GeofenceRadiusMeters,
    string? Timezone);
