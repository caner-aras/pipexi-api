using Workforce.Application.Features.Locations.Dtos;
using Workforce.Domain.Entities;

namespace Workforce.Application.Features.Locations;

internal static class LocationMappings
{
    public static LocationDto ToDto(this Location location, IReadOnlyCollection<LocationWorkingHourDto>? workingHours = null)
    {
        return new LocationDto(
            location.Id,
            location.OrganizationId,
            location.Name,
            location.Address,
            location.Latitude,
            location.Longitude,
            location.GeofenceRadiusMeters,
            location.Timezone,
            location.Status,
            location.CreatedAt,
            location.UpdatedAt,
            workingHours ?? Array.Empty<LocationWorkingHourDto>());
    }

    public static LocationWorkingHourDto ToDto(this LocationWorkingHour workingHour)
    {
        return new LocationWorkingHourDto(
            workingHour.Id,
            workingHour.LocationId,
            workingHour.DayOfWeek,
            workingHour.IsClosed,
            workingHour.OpensAt,
            workingHour.ClosesAt,
            workingHour.Status,
            workingHour.CreatedAt,
            workingHour.UpdatedAt);
    }
}
