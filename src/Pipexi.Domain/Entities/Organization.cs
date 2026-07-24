using Pipexi.Domain.Time;

namespace Pipexi.Domain.Entities;

public sealed class Organization : BaseEntity
{
    private Organization(
        Guid id,
        string name,
        string slug,
        string timezone,
        string currency,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        Name = name;
        Slug = slug;
        Timezone = NormalizeTimezone(timezone);
        Currency = NormalizeCurrency(currency);
        UpdatedAt = updatedAt;
    }

    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string Timezone { get; private set; }
    public string Currency { get; private set; }

    public static Organization Create(string name, string slug, string timezone, string? currency = null)
    {
        return new Organization(
            Guid.NewGuid(),
            name.Trim(),
            slug.Trim().ToLowerInvariant(),
            timezone,
            currency ?? "USD",
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(string? name, string? slug, string? timezone, string? currency, string? status)
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

        if (currency is not null)
        {
            Currency = NormalizeCurrency(currency);
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (name is not null || slug is not null || timezone is not null || currency is not null || status is not null)
        {
            Touch();
        }
    }

    private static string NormalizeCurrency(string currency)
    {
        var normalized = currency.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length != 3)
        {
            return "USD";
        }

        return normalized;
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
