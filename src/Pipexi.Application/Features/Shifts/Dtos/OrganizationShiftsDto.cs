using Workforce.Application.Features.Locations.Dtos;
using Workforce.Application.Features.OrganizationMembers.Dtos;
using Workforce.Application.Features.Teams.Dtos;
using Workforce.Application.Features.TimeEntries.Dtos;

namespace Workforce.Application.Features.Shifts.Dtos;

public sealed record OrganizationShiftsDto(
    Guid OrganizationId,
    IReadOnlyCollection<LocationDto> Locations,
    IReadOnlyCollection<OrganizationShiftDto> Shifts);

public sealed record OrganizationShiftDto(
    Guid Id,
    Guid OrganizationId,
    TeamDto? Team,
    Guid? OrganizationMemberId,
    OrganizationMemberDto? OrganizationMember,
    Guid LocationId,
    string? Title,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string? Notes,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyCollection<ShiftBreakDto> Breaks,
    IReadOnlyCollection<TimeEntryDto> TimeEntries);