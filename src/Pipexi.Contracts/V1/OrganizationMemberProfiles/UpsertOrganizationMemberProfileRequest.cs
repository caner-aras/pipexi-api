namespace Pipexi.Contracts.V1.OrganizationMemberProfiles;

public sealed record UpsertOrganizationMemberProfileRequest(
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
    string? NationalId);
