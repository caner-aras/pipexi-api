using Workforce.Application.Features.OrganizationMembers.Dtos;
using Workforce.Domain.Entities;

namespace Workforce.Application.Features.OrganizationMembers;

internal static class OrganizationMemberMappings
{
    public static OrganizationMemberDto ToDto(
        this OrganizationMember organizationMember,
        OrganizationMemberUserDto? user = null)
    {
        return new OrganizationMemberDto(
            organizationMember.Id,
            organizationMember.OrganizationId,
            organizationMember.UserId,
            organizationMember.RoleId,
            organizationMember.JobTitle,
            organizationMember.Status,
            organizationMember.CreatedAt,
            organizationMember.UpdatedAt,
            user);
    }

    public static OrganizationMemberUserDto ToDto(this User user)
    {
        return new OrganizationMemberUserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Phone,
            user.AvatarUrl);
    }
}
