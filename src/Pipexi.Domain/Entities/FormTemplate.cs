namespace Workforce.Domain.Entities;

public sealed class FormTemplate : BaseEntity
{
    private FormTemplate(
        Guid id,
        Guid organizationId,
        string name,
        string? description,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationId = organizationId;
        Name = name;
        Description = description;
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }

    public static FormTemplate Create(Guid organizationId, string name, string? description)
    {
        return new FormTemplate(
            Guid.NewGuid(),
            organizationId,
            name.Trim(),
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(string? name, string? description, string? status)
    {
        if (name is not null)
        {
            Name = name.Trim();
        }

        if (description is not null)
        {
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (name is not null || description is not null || status is not null)
        {
            Touch();
        }
    }
}
