using Workforce.Application.Features.LeaveRequests.Dtos;
using Workforce.Application.Features.OrganizationMembers.Dtos;
using Workforce.Application.Features.Organizations.Dtos;
using Workforce.Domain.Entities;

namespace Workforce.Application.Features.LeaveRequests;

internal static class LeaveRequestMappings
{
    public static LeaveRequestDto ToDto(
        this LeaveRequest leaveRequest,
        OrganizationDto? organization = null,
        OrganizationMemberDto? organizationMember = null)
    {
        return new LeaveRequestDto(
            leaveRequest.Id,
            leaveRequest.OrganizationId,
            leaveRequest.OrganizationMemberId,
            leaveRequest.LeaveType,
            leaveRequest.StartDate,
            leaveRequest.EndDate,
            leaveRequest.Status,
            leaveRequest.Reason,
            leaveRequest.CreatedAt,
            leaveRequest.UpdatedAt,
            organization,
            organizationMember);
    }
}
