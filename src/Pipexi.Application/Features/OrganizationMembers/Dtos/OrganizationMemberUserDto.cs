namespace Workforce.Application.Features.OrganizationMembers.Dtos;

public sealed record OrganizationMemberUserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string? AvatarUrl);
