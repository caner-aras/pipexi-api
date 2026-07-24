namespace Pipexi.Domain.Entities;

public sealed class Position : BaseEntity
{
    private Position(
        Guid id,
        Guid organizationId,
        string title,
        string? description,
        decimal defaultHourlyRate,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationId = organizationId;
        Title = title;
        Description = description;
        DefaultHourlyRate = defaultHourlyRate;
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public decimal DefaultHourlyRate { get; private set; }

    public static Position Create(
        Guid organizationId,
        string title,
        decimal defaultHourlyRate,
        string? description = null)
    {
        return new Position(
            Guid.NewGuid(),
            organizationId,
            title.Trim(),
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            defaultHourlyRate,
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(
        string? title,
        string? description,
        decimal? defaultHourlyRate,
        string? status)
    {
        if (title is not null)
        {
            Title = title.Trim();
        }

        if (description is not null)
        {
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }

        if (defaultHourlyRate.HasValue)
        {
            DefaultHourlyRate = defaultHourlyRate.Value;
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (title is not null || description is not null || defaultHourlyRate.HasValue || status is not null)
        {
            Touch();
        }
    }
}
