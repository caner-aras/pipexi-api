using Workforce.Application.Features.Forms.Dtos;
using Workforce.Application.Features.Locations.Dtos;
using Workforce.Application.Features.OrganizationMembers.Dtos;
using Workforce.Application.Features.Teams.Dtos;
using Workforce.Application.Features.TimeEntries.Dtos;

namespace Workforce.Application.Features.Shifts.Dtos;

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
