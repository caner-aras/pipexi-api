using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Application.Features.Locations.Dtos;
using Pipexi.Application.Features.OrganizationMembers.Dtos;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Application.Features.TimeEntries.Dtos;

namespace Pipexi.Application.Features.Shifts.Dtos;

public sealed record ShiftDto(
    Guid Id,
    Guid OrganizationId,
    TeamDto? Team,
    Guid? OrganizationMemberId,
    OrganizationMemberDto? OrganizationMember,
    LocationDto? Location,
    string? Title,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string? Notes,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyCollection<ShiftBreakDto> Breaks,
    IReadOnlyCollection<TimeEntryDto> TimeEntries,
    IReadOnlyCollection<ShiftFormTemplateDto> ShiftFormTemplates);
