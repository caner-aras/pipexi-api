using Pipexi.Domain.Time;

namespace Pipexi.Domain.Entities;

public sealed class Location : BaseEntity
{
    private Location(
        Guid id,
        Guid organizationId,
        string name,
        string? address,
        decimal? latitude,
        decimal? longitude,
        int geofenceRadiusMeters,
        string? timezone,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationId = organizationId;
        Name = name;
        Address = address;
        Latitude = latitude;
        Longitude = longitude;
        GeofenceRadiusMeters = geofenceRadiusMeters;
        Timezone = NormalizeTimezone(timezone);
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; }
    public string? Address { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public int GeofenceRadiusMeters { get; private set; }
    public string? Timezone { get; private set; }

    public static Location Create(
        Guid organizationId,
        string name,
        string? address,
        decimal? latitude,
        decimal? longitude,
        int geofenceRadiusMeters,
        string? timezone)
    {
        return new Location(
            Guid.NewGuid(),
            organizationId,
            name.Trim(),
            NormalizeNullableString(address),
            latitude,
            longitude,
            geofenceRadiusMeters,
            timezone,
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(
        string? name,
        string? address,
        decimal? latitude,
        decimal? longitude,
        int? geofenceRadiusMeters,
        string? timezone,
        string? status)
    {
        if (name is not null)
        {
            Name = name.Trim();
        }

        if (address is not null)
        {
            Address = NormalizeNullableString(address);
        }

        if (latitude.HasValue)
        {
            Latitude = latitude.Value;
        }

        if (longitude.HasValue)
        {
            Longitude = longitude.Value;
        }

        if (geofenceRadiusMeters.HasValue)
        {
            GeofenceRadiusMeters = geofenceRadiusMeters.Value;
        }

        if (timezone is not null)
        {
            Timezone = NormalizeTimezone(timezone);
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (name is not null ||
            address is not null ||
            latitude.HasValue ||
            longitude.HasValue ||
            geofenceRadiusMeters.HasValue ||
            timezone is not null ||
            status is not null)
        {
            Touch();
        }
    }

    private static string? NormalizeNullableString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static string? NormalizeTimezone(string? timezone)
    {
        if (string.IsNullOrWhiteSpace(timezone))
        {
            return null;
        }

        var normalizedTimezone = timezone.Trim();
        if (!IanaTimeZone.IsValid(normalizedTimezone))
        {
            throw new ArgumentException("Timezone is not valid.", nameof(timezone));
        }

        return normalizedTimezone;
    }
}
