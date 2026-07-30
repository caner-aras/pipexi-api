namespace Pipexi.Application.Abstractions.Identity;

public sealed record CurrentUserMembership(
    Guid OrganizationId,
    Guid OrganizationMemberId,
    Guid RoleId,
    string RoleName);
