namespace Workforce.Domain.Entities;

public sealed class RolePermission : BaseEntity
{
    private RolePermission(
        Guid id,
        Guid roleId,
        Guid permissionId,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        RoleId = roleId;
        PermissionId = permissionId;
        UpdatedAt = updatedAt;
    }

    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }

    public static RolePermission Create(Guid roleId, Guid permissionId)
    {
        return new RolePermission(
            Guid.NewGuid(),
            roleId,
            permissionId,
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(string? status)
    {
        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
            Touch();
        }
    }
}
