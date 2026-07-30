namespace Pipexi.Application.Features.OrganizationMemberProfiles.Dtos;

public sealed record OrganizationMemberProfileDto(
    Guid Id,
    Guid OrganizationMemberId,
    DateOnly? DateOfBirth,
    string? Gender,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? NationalId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
