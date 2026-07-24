using Pipexi.Application.Features.Shifts.Dtos;
using Pipexi.Application.Features.TimeEntries.Dtos;

namespace Pipexi.Application.Features.Teams.Dtos;

public sealed record TeamMemberDetailsDto(
    TeamMemberDto TeamMember,
    Guid OrganizationId,
    string? OrganizationName,
    IReadOnlyCollection<ShiftDto> Shifts,
    IReadOnlyCollection<TimeEntryDto> TimeEntries,
    int TotalTaskCount);
