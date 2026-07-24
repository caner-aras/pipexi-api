namespace Pipexi.Contracts.V1.Roles;

public sealed record CreateRoleRequest(Guid OrganizationId, string Name);
