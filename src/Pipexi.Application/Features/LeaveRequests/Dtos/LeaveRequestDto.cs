using Workforce.Application.Features.OrganizationMembers.Dtos;
using Workforce.Application.Features.Organizations.Dtos;

namespace Workforce.Application.Features.LeaveRequests.Dtos;

public sealed record LeaveRequestDto(
    Guid Id,
    Guid OrganizationId,
    Guid OrganizationMemberId,
    string LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    string Reason,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    OrganizationDto? Organization = null,
    OrganizationMemberDto? OrganizationMember = null);
