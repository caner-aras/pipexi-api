namespace Workforce.Contracts.V1.OrganizationMembers;

public sealed record CreateOrganizationMemberRequest(
    Guid OrganizationId,
    Guid UserId,
    Guid RoleId,
    string? JobTitle);
