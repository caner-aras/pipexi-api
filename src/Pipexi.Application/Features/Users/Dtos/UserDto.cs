namespace Workforce.Application.Features.Users.Dtos;

public sealed record UserDto(
    Guid Id,
    string AuthProviderId,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string? AvatarUrl);
