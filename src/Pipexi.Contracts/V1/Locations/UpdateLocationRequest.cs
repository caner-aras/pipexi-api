namespace Pipexi.Contracts.V1.Locations;

public sealed record UpdateLocationRequest(
    string? Name,
    string? Address,
    decimal? Latitude,
    decimal? Longitude,
    int? GeofenceRadiusMeters,
    string? Timezone,
    string? Status);
