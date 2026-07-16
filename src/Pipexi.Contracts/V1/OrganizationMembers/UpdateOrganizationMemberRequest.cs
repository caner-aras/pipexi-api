namespace Workforce.Contracts.V1.OrganizationMembers;

public sealed record UpdateOrganizationMemberRequest(
    Guid? RoleId,
    string? JobTitle,
    string? Status);
