using Workforce.Application.Features.Shifts.Dtos;
using Workforce.Application.Features.TimeEntries.Dtos;

namespace Workforce.Application.Features.Teams.Dtos;

public sealed record TeamMemberDetailsDto(
    TeamMemberDto TeamMember,
    Guid OrganizationId,
    string? OrganizationName,
    IReadOnlyCollection<ShiftDto> Shifts,
    IReadOnlyCollection<TimeEntryDto> TimeEntries,
    int TotalTaskCount);
