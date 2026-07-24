namespace Pipexi.Domain.Entities;

public sealed class Role : BaseEntity
{
    private Role(
        Guid id,
        Guid organizationId,
        string name,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationId = organizationId;
        Name = name;
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; }

    public static Role Create(Guid organizationId, string name)
    {
        return new Role(
            Guid.NewGuid(),
            organizationId,
            name.Trim(),
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(string? name, string? status)
    {
        if (name is not null)
        {
            Name = name.Trim();
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (name is not null || status is not null)
        {
            Touch();
        }
    }
}
