namespace Pipexi.Domain.Entities;

public sealed class OrganizationMemberProfile : BaseEntity
{
    private OrganizationMemberProfile(
        Guid id,
        Guid organizationMemberId,
        DateOnly? dateOfBirth,
        string? gender,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? state,
        string? postalCode,
        string? country,
        string? emergencyContactName,
        string? emergencyContactPhone,
        string? nationalId,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationMemberId = organizationMemberId;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
        EmergencyContactName = emergencyContactName;
        EmergencyContactPhone = emergencyContactPhone;
        NationalId = nationalId;
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationMemberId { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public string? Gender { get; private set; }
    public string? AddressLine1 { get; private set; }
    public string? AddressLine2 { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Country { get; private set; }
    public string? EmergencyContactName { get; private set; }
    public string? EmergencyContactPhone { get; private set; }
    public string? NationalId { get; private set; }

    public static OrganizationMemberProfile Create(
        Guid organizationMemberId,
        DateOnly? dateOfBirth = null,
        string? gender = null,
        string? addressLine1 = null,
        string? addressLine2 = null,
        string? city = null,
        string? state = null,
        string? postalCode = null,
        string? country = null,
        string? emergencyContactName = null,
        string? emergencyContactPhone = null,
        string? nationalId = null)
    {
        return new OrganizationMemberProfile(
            Guid.NewGuid(),
            organizationMemberId,
            dateOfBirth,
            NormalizeOptional(gender)?.ToLowerInvariant(),
            NormalizeOptional(addressLine1),
            NormalizeOptional(addressLine2),
            NormalizeOptional(city),
            NormalizeOptional(state),
            NormalizeOptional(postalCode),
            NormalizeOptional(country),
            NormalizeOptional(emergencyContactName),
            NormalizeOptional(emergencyContactPhone),
            NormalizeOptional(nationalId),
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(
        DateOnly? dateOfBirth,
        string? gender,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? state,
        string? postalCode,
        string? country,
        string? emergencyContactName,
        string? emergencyContactPhone,
        string? nationalId)
    {
        DateOfBirth = dateOfBirth;
        Gender = NormalizeOptional(gender)?.ToLowerInvariant();
        AddressLine1 = NormalizeOptional(addressLine1);
        AddressLine2 = NormalizeOptional(addressLine2);
        City = NormalizeOptional(city);
        State = NormalizeOptional(state);
        PostalCode = NormalizeOptional(postalCode);
        Country = NormalizeOptional(country);
        EmergencyContactName = NormalizeOptional(emergencyContactName);
        EmergencyContactPhone = NormalizeOptional(emergencyContactPhone);
        NationalId = NormalizeOptional(nationalId);
        Touch();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
