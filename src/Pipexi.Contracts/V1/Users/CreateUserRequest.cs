namespace Workforce.Contracts.V1.Users;

public sealed record CreateUserRequest(
    string? AuthProviderId,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string? AvatarUrl);
