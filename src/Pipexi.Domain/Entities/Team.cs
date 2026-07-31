namespace Pipexi.Domain.Entities;

public sealed class Team : BaseEntity
{
    private Team(
        Guid id,
        Guid organizationId,
        string name,
        Guid? managerMemberId,
        Guid? locationId,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationId = organizationId;
        Name = name;
        ManagerMemberId = managerMemberId;
        LocationId = locationId;
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; }
    public Guid? ManagerMemberId { get; private set; }
    public Guid? LocationId { get; private set; }

    public static Team Create(
        Guid organizationId,
        string name,
        Guid? managerMemberId,
        Guid? locationId = null)
    {
        return new Team(
            Guid.NewGuid(),
            organizationId,
            name.Trim(),
            managerMemberId,
            locationId,
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(
        string? name,
        Guid? managerMemberId,
        string? status,
        Guid? locationId,
        bool updateLocation)
    {
        if (name is not null)
        {
            Name = name.Trim();
        }

        if (managerMemberId.HasValue)
        {
            ManagerMemberId = managerMemberId.Value;
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (updateLocation)
        {
            LocationId = locationId;
        }

        if (name is not null ||
            managerMemberId.HasValue ||
            status is not null ||
            updateLocation)
        {
            Touch();
        }
    }

    public void ClearManager()
    {
        ManagerMemberId = null;
        Touch();
    }
}
