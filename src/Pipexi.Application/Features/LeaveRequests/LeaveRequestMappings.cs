using Pipexi.Application.Features.LeaveRequests.Dtos;
using Pipexi.Application.Features.OrganizationMembers.Dtos;
using Pipexi.Application.Features.Organizations.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.LeaveRequests;

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
