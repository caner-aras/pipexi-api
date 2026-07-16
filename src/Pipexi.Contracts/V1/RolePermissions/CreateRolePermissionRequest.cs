namespace Workforce.Contracts.V1.RolePermissions;

public sealed record CreateRolePermissionRequest(Guid RoleId, Guid PermissionId);
