using Pipexi.Application.Features.Locations.Dtos;
using Pipexi.Application.Features.Shifts.Dtos;
using Pipexi.Application.Features.Teams.Dtos;

namespace Pipexi.Application.Features.Tasks.Dtos;

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
    TaskCommentMemberUserDto? Reporter = null,
    LocationDto? Location = null,
    ShiftDto? Shift = null);
