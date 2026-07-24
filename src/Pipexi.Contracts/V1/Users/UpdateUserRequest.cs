namespace Pipexi.Contracts.V1.Users;

public sealed record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    string? Phone,
    string? AvatarUrl);
