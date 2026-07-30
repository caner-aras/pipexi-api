using Pipexi.Application.Features.OrganizationMemberProfiles.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.OrganizationMemberProfiles;

internal static class OrganizationMemberProfileMappings
{
    public static OrganizationMemberProfileDto ToDto(this OrganizationMemberProfile profile)
    {
        return new OrganizationMemberProfileDto(
            profile.Id,
            profile.OrganizationMemberId,
            profile.DateOfBirth,
            profile.Gender,
            profile.AddressLine1,
            profile.AddressLine2,
            profile.City,
            profile.State,
            profile.PostalCode,
            profile.Country,
            profile.EmergencyContactName,
            profile.EmergencyContactPhone,
            profile.NationalId,
            profile.Status,
            profile.CreatedAt,
            profile.UpdatedAt);
    }
}
