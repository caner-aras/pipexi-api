using Workforce.Domain.Time;

namespace Workforce.Domain.Entities;

public sealed class Organization : BaseEntity
{
    private Organization(
        Guid id,
        string name,
        string slug,
        string timezone,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        Name = name;
        Slug = slug;
        Timezone = NormalizeTimezone(timezone);
        UpdatedAt = updatedAt;
    } 

    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string Timezone { get; private set; }

    public static Organization Create(string name, string slug, string timezone)
    {
        return new Organization(
            Guid.NewGuid(),
            name.Trim(),
            slug.Trim().ToLowerInvariant(),
            timezone,
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(string? name, string? slug, string? timezone, string? status)
    {
        if (name is not null)
        {
            Name = name.Trim();
        }

        if (slug is not null)
        {
            Slug = slug.Trim().ToLowerInvariant();
        }

        if (timezone is not null)
        {
            Timezone = NormalizeTimezone(timezone);
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (name is not null || slug is not null || timezone is not null || status is not null)
        {
            Touch();
        }
    }

    private static bool IsValidTimezone(string timezone)
    {
        return IanaTimeZone.IsValid(timezone);
    }

    private static string NormalizeTimezone(string timezone)
    {
        var normalizedTimezone = timezone.Trim();
        if (!IsValidTimezone(normalizedTimezone))
        {
            throw new ArgumentException(
                "Timezone is not valid.",
                nameof(timezone));
        }

        return normalizedTimezone;
    }
}
