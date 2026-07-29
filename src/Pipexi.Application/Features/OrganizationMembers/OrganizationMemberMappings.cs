using Pipexi.Application.Common;
using Pipexi.Application.Features.OrganizationMembers.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.OrganizationMembers;

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
            AvatarUrls.Resolve(user.Id, user.AvatarUrl));
    }
}
