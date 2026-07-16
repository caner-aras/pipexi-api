namespace Workforce.Contracts.V1.Teams;

public sealed record CreateTeamMemberWithUserRequest(
    string Email,
    string FirstName,
    string LastName,
    Guid RoleId,
    string? JobTitle,
    string? Phone,
    string? AvatarUrl,
    string? AuthProviderId = null);
