namespace Pipexi.Application.Abstractions.Identity;

/// <summary>
/// Per-request hydrated membership (org + role) from DB.
/// Populated by API middleware after JWT authentication.
/// </summary>
public sealed class CurrentUserMembershipState
{
    public bool IsResolved { get; private set; }

    public Guid OrganizationId { get; private set; }

    public Guid? OrganizationMemberId { get; private set; }

    public Guid? RoleId { get; private set; }

    public string Role { get; private set; } = string.Empty;

    public void Set(CurrentUserMembership membership)
    {
        OrganizationId = membership.OrganizationId;
        OrganizationMemberId = membership.OrganizationMemberId;
        RoleId = membership.RoleId;
        Role = membership.RoleName;
        IsResolved = true;
    }

    public void Clear()
    {
        OrganizationId = Guid.Empty;
        OrganizationMemberId = null;
        RoleId = null;
        Role = string.Empty;
        IsResolved = false;
    }
}
