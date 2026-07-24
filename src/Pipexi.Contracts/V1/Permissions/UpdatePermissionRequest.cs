namespace Pipexi.Contracts.V1.Permissions;

public sealed record UpdatePermissionRequest(
    string? Key,
    string? Status);
