namespace Workforce.Application.Features.Organizations.Provisioning;

public static class OrganizationProvisioningDefaults
{
    public static readonly OrganizationRoleType[] StaticRoles =
    [
        OrganizationRoleType.Owner,
        OrganizationRoleType.Sales,
        OrganizationRoleType.HumanResources,
        OrganizationRoleType.Manager,
        OrganizationRoleType.User
    ];

    public static string ToRoleName(this OrganizationRoleType role)
    {
        return role switch
        {
            OrganizationRoleType.Owner => "owner",
            OrganizationRoleType.Sales => "sales",
            OrganizationRoleType.HumanResources => "human_resources",
            OrganizationRoleType.Manager => "manager",
            OrganizationRoleType.User => "user",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown organization role type.")
        };
    }
}
