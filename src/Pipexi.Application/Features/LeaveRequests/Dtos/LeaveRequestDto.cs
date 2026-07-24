using Pipexi.Application.Features.OrganizationMembers.Dtos;
using Pipexi.Application.Features.Organizations.Dtos;

namespace Pipexi.Application.Features.LeaveRequests.Dtos;

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
