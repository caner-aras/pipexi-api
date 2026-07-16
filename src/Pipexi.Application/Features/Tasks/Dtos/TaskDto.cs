using Workforce.Application.Features.Locations.Dtos;
using Workforce.Application.Features.Shifts.Dtos;
using Workforce.Application.Features.Teams.Dtos;

namespace Workforce.Application.Features.Tasks.Dtos;

public sealed record TaskDto(
    Guid Id,
    Guid OrganizationId,
    Guid? ReporterUserId,
    Guid? ShiftId,
    Guid? LocationId,
    string Title,
    string? Description,
    Guid? AssignedToTeamMemberId,
    Guid? AssignedToTeamId,
    DateTimeOffset? DueAt,
    string Priority,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyCollection<TaskCommentDto> Comments,
    TeamMemberDto? AssignedToTeamMember = null,
    LocationDto? Location = null,
    ShiftDto? Shift = null);
