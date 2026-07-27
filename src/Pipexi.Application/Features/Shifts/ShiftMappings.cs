using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Application.Features.Locations.Dtos;
using Pipexi.Application.Features.OrganizationMembers.Dtos;
using Pipexi.Application.Features.Shifts.Dtos;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Application.Features.TimeEntries.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.Shifts;

internal static class ShiftMappings
{
    public static ShiftDto ToDto(
        this Shift shift,
        TeamDto? team = null,
        OrganizationMemberDto? organizationMember = null,
        LocationDto? location = null,
        IReadOnlyCollection<ShiftBreakDto>? breaks = null,
        IReadOnlyCollection<TimeEntryDto>? timeEntries = null,
        IReadOnlyCollection<ShiftFormTemplateDto>? shiftFormTemplates = null,
        Guid? teamMemberId = null)
    {
        return new ShiftDto(
            shift.Id,
            shift.OrganizationId,
            team,
            shift.OrganizationMemberId,
            organizationMember,
            teamMemberId,
            location,
            shift.Title,
            shift.StartAt,
            shift.EndAt,
            shift.Notes,
            shift.Status,
            shift.CreatedAt,
            shift.UpdatedAt,
            breaks ?? Array.Empty<ShiftBreakDto>(),
            timeEntries ?? Array.Empty<TimeEntryDto>(),
            shiftFormTemplates ?? Array.Empty<ShiftFormTemplateDto>());
    }

    public static ShiftBreakDto ToDto(this ShiftBreak shiftBreak)
    {
        return new ShiftBreakDto(
            shiftBreak.Id,
            shiftBreak.ShiftId,
            shiftBreak.StartAt,
            shiftBreak.EndAt,
            shiftBreak.IsPaid,
            shiftBreak.Status,
            shiftBreak.CreatedAt,
            shiftBreak.UpdatedAt);
    }

    public static OrganizationShiftDto ToOrganizationShiftDto(
        this Shift shift,
        TeamDto? team = null,
        OrganizationMemberDto? organizationMember = null,
        IReadOnlyCollection<ShiftBreakDto>? breaks = null,
        IReadOnlyCollection<TimeEntryDto>? timeEntries = null,
        Guid? teamMemberId = null)
    {
        return new OrganizationShiftDto(
            shift.Id,
            shift.OrganizationId,
            team,
            shift.OrganizationMemberId,
            organizationMember,
            teamMemberId,
            shift.LocationId,
            shift.Title,
            shift.StartAt,
            shift.EndAt,
            shift.Notes,
            shift.Status,
            shift.CreatedAt,
            shift.UpdatedAt,
            breaks ?? Array.Empty<ShiftBreakDto>(),
            timeEntries ?? Array.Empty<TimeEntryDto>());
    }
}
