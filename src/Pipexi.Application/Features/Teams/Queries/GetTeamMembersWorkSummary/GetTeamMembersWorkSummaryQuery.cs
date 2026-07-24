using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Application.Features.TimeEntries;
using Pipexi.Application.Features.TimeEntries.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Queries.GetTeamMembersWorkSummary;

public sealed record GetTeamMembersWorkSummaryQuery(
    Guid TeamMemberId,
    DateOnly FromDate,
    DateOnly ToDate,
    Guid? OrganizationId = null) : IQuery<Result<TeamMembersWorkSummaryDto>>
{
    public sealed class Handler : IRequestHandler<GetTeamMembersWorkSummaryQuery, Result<TeamMembersWorkSummaryDto>>
    {
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IShiftRepository _shiftRepository;
        private readonly IShiftBreakRepository _shiftBreakRepository;
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly ITimeEntryBreakRepository _timeEntryBreakRepository;
        private readonly IMemberPositionHistoryRepository _memberPositionHistoryRepository;
        private readonly IOrganizationRepository _organizationRepository;

        public Handler(
            ITeamMemberRepository teamMemberRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IShiftRepository shiftRepository,
            IShiftBreakRepository shiftBreakRepository,
            ITimeEntryRepository timeEntryRepository,
            ITimeEntryBreakRepository timeEntryBreakRepository,
            IMemberPositionHistoryRepository memberPositionHistoryRepository,
            IOrganizationRepository organizationRepository)
        {
            _teamMemberRepository = teamMemberRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _shiftRepository = shiftRepository;
            _shiftBreakRepository = shiftBreakRepository;
            _timeEntryRepository = timeEntryRepository;
            _timeEntryBreakRepository = timeEntryBreakRepository;
            _memberPositionHistoryRepository = memberPositionHistoryRepository;
            _organizationRepository = organizationRepository;
        }

        public async Task<Result<TeamMembersWorkSummaryDto>> Handle(
            GetTeamMembersWorkSummaryQuery request,
            CancellationToken cancellationToken)
        {
            if (request.FromDate > request.ToDate)
            {
                return Result<TeamMembersWorkSummaryDto>.Failure(
                    new AppError("team_members.work_summary.invalid_date_range", "fromDate must be before or equal to toDate."),
                    (int)HttpStatusCode.BadRequest);
            }

            var teamMember = await _teamMemberRepository.GetByIdAsync(request.TeamMemberId, cancellationToken);
            if (teamMember is null)
            {
                return Result<TeamMembersWorkSummaryDto>.Failure(
                    new AppError("team_members.not_found", "Team member not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var organizationMembers = await _organizationMemberRepository.GetByIdsAsync(
                new[] { teamMember.OrganizationMemberId },
                cancellationToken);
            var organizationMemberMap = organizationMembers.ToDictionary(x => x.Id);

            if (request.OrganizationId.HasValue)
            {
                var hasOutOfScope =
                    !organizationMemberMap.TryGetValue(teamMember.OrganizationMemberId, out var om) ||
                    om.OrganizationId != request.OrganizationId.Value;

                if (hasOutOfScope)
                {
                    return Result<TeamMembersWorkSummaryDto>.Failure(
                        new AppError("team_members.work_summary.organization_mismatch", "One or more team members do not belong to the organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            var fromStart = request.FromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var toEndExclusive = request.ToDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var fromDateTime = new DateTimeOffset(fromStart);
            var toDateTimeExclusive = new DateTimeOffset(toEndExclusive);

            var days = BuildDays(request.FromDate, request.ToDate);
            var members = new List<TeamMemberWorkSummaryDto>(1);
            var orgMemberId = teamMember.OrganizationMemberId;

            var shifts = await _shiftRepository.ListByOrganizationMemberIdAsync(orgMemberId, cancellationToken);
            shifts = shifts
                .Where(x => x.StartAt >= fromDateTime && x.StartAt < toDateTimeExclusive)
                .Where(x => !request.OrganizationId.HasValue || x.OrganizationId == request.OrganizationId.Value)
                .OrderBy(x => x.StartAt)
                .ToList();

            var shiftIds = shifts.Select(x => x.Id).ToList();
            var shiftBreakMap = new Dictionary<Guid, IReadOnlyCollection<Pipexi.Domain.Entities.ShiftBreak>>();
            if (shiftIds.Count > 0)
            {
                var shiftBreaks = await _shiftBreakRepository.ListByShiftIdsAsync(shiftIds, cancellationToken);
                shiftBreakMap = shiftBreaks
                    .GroupBy(x => x.ShiftId)
                    .ToDictionary(g => g.Key, g => (IReadOnlyCollection<Pipexi.Domain.Entities.ShiftBreak>)g.ToList());
            }

            var timeEntries = await _timeEntryRepository.ListByOrganizationMemberIdAsync(orgMemberId, cancellationToken);
            timeEntries = timeEntries
                .Where(x => x.ClockInAt >= fromDateTime && x.ClockInAt < toDateTimeExclusive)
                .Where(x => !request.OrganizationId.HasValue || x.OrganizationId == request.OrganizationId.Value)
                .OrderBy(x => x.ClockInAt)
                .ToList();

            var timeEntryIds = timeEntries.Select(x => x.Id).ToList();
            var timeEntryBreakMap = new Dictionary<Guid, IReadOnlyCollection<Pipexi.Domain.Entities.TimeEntryBreak>>();
            if (timeEntryIds.Count > 0)
            {
                var timeEntryBreaks = await _timeEntryBreakRepository.ListByTimeEntryIdsAsync(timeEntryIds, cancellationToken);
                timeEntryBreakMap = timeEntryBreaks
                    .GroupBy(x => x.TimeEntryId)
                    .ToDictionary(g => g.Key, g => (IReadOnlyCollection<Pipexi.Domain.Entities.TimeEntryBreak>)g.ToList());
            }

            var timeEntriesByShift = timeEntries
                .GroupBy(x => x.ShiftId)
                .ToDictionary(g => g.Key, g => (IReadOnlyCollection<Pipexi.Domain.Entities.TimeEntry>)g.ToList());

            var dailyRows = new List<TeamMemberWorkDayDto>(days.Count);
            foreach (var day in days)
            {
                var dayShifts = shifts
                    .Where(x => DateOnly.FromDateTime(x.StartAt.UtcDateTime) == day)
                    .ToList();

                var shiftRows = dayShifts
                    .Select(shift =>
                    {
                        var plannedShiftMinutes = GetMinutes(shift.StartAt, shift.EndAt);
                        var shiftBreaks = shiftBreakMap.GetValueOrDefault(shift.Id) ?? Array.Empty<Pipexi.Domain.Entities.ShiftBreak>();
                        var plannedBreakMinutes = shiftBreaks.Sum(x => GetMinutes(x.StartAt, x.EndAt));

                        var linkedTimeEntries = timeEntriesByShift.GetValueOrDefault(shift.Id) ?? Array.Empty<Pipexi.Domain.Entities.TimeEntry>();
                        var linkedTimeEntryDtos = linkedTimeEntries
                            .Select(te => te.ToDto(
                                (timeEntryBreakMap.GetValueOrDefault(te.Id) ?? Array.Empty<Pipexi.Domain.Entities.TimeEntryBreak>())
                                .OrderBy(x => x.StartAt)
                                .Select(x => x.ToDto())
                                .ToList()))
                            .ToList();

                        var timeEntryMinutes = linkedTimeEntries.Sum(te =>
                            GetMinutes(te.ClockInAt, te.ClockOutAt ?? te.ClockInAt));

                        var timeEntryBreakMinutes = linkedTimeEntries.Sum(te =>
                            (timeEntryBreakMap.GetValueOrDefault(te.Id) ?? Array.Empty<Pipexi.Domain.Entities.TimeEntryBreak>())
                            .Sum(tb => GetMinutes(tb.StartAt, tb.EndAt)));

                        var workedMinutes = Math.Max(0, timeEntryMinutes - timeEntryBreakMinutes);

                        return new TeamMemberWorkShiftDto(
                            shift.Id,
                            shift.StartAt,
                            shift.EndAt,
                            plannedShiftMinutes,
                            plannedBreakMinutes,
                            timeEntryMinutes,
                            timeEntryBreakMinutes,
                            workedMinutes,
                            linkedTimeEntryDtos);
                    })
                    .ToList();

                dailyRows.Add(new TeamMemberWorkDayDto(day, shiftRows));
            }

            var totalShiftDurationMinutes = dailyRows
                .SelectMany(x => x.Shifts)
                .Sum(x => x.ShiftDurationMinutes);
            var totalShiftBreakDurationMinutes = dailyRows
                .SelectMany(x => x.Shifts)
                .Sum(x => x.ShiftBreakDurationMinutes);
            var totalTimeEntryDurationMinutes = dailyRows
                .SelectMany(x => x.Shifts)
                .Sum(x => x.TimeEntryDurationMinutes);
            var totalTimeEntryBreakDurationMinutes = dailyRows
                .SelectMany(x => x.Shifts)
                .Sum(x => x.TimeEntryBreakDurationMinutes);
            var totalWorkedDurationMinutes = dailyRows
                .SelectMany(x => x.Shifts)
                .Sum(x => x.WorkedDurationMinutes);

            var totalTimeEntryCount = dailyRows
                .SelectMany(x => x.Shifts)
                .SelectMany(x => x.TimeEntries)
                .Count();

            var org = await _organizationRepository.GetByIdAsync(
                organizationMemberMap.TryGetValue(teamMember.OrganizationMemberId, out var omember)
                    ? omember.OrganizationId
                    : request.OrganizationId ?? Guid.Empty,
                cancellationToken);
            var currency = org?.Currency ?? "USD";

            var activePosition = await _memberPositionHistoryRepository.GetActiveByOrganizationMemberIdAsync(orgMemberId, cancellationToken);
            decimal? totalEarnings = activePosition != null
                ? Math.Round((totalWorkedDurationMinutes / 60m) * activePosition.HourlyRate, 2, MidpointRounding.AwayFromZero)
                : null;

            var totals = new TeamMemberWorkTotalsDto(
                dailyRows.Sum(x => x.Shifts.Count),
                ToHours(totalShiftDurationMinutes),
                ToHours(totalShiftBreakDurationMinutes),
                totalTimeEntryCount,
                ToHours(totalTimeEntryDurationMinutes),
                ToHours(totalTimeEntryBreakDurationMinutes),
                ToHours(totalWorkedDurationMinutes),
                ToHourText(totalShiftDurationMinutes),
                ToHourText(totalTimeEntryBreakDurationMinutes),
                ToHourText(totalWorkedDurationMinutes),
                totalEarnings,
                currency);

            members.Add(new TeamMemberWorkSummaryDto(
                teamMember.Id,
                orgMemberId,
                teamMember.Status,
                dailyRows,
                totals));

            return Result<TeamMembersWorkSummaryDto>.Success(
                new TeamMembersWorkSummaryDto(request.FromDate, request.ToDate, members));
        }

        private static List<DateOnly> BuildDays(DateOnly fromDate, DateOnly toDate)
        {
            var days = new List<DateOnly>();
            for (var d = fromDate; d <= toDate; d = d.AddDays(1))
            {
                days.Add(d);
            }

            return days;
        }

        private static int GetMinutes(DateTimeOffset startAt, DateTimeOffset endAt)
        {
            var minutes = (int)Math.Floor((endAt - startAt).TotalMinutes);
            return Math.Max(0, minutes);
        }

        private static decimal ToHours(int totalMinutes)
        {
            var safe = Math.Max(0, totalMinutes);
            return Math.Round(safe / 60m, 2, MidpointRounding.AwayFromZero);
        }

        private static string ToHourText(int totalMinutes)
        {
            var safe = Math.Max(0, totalMinutes);
            var hours = safe / 60;
            var minutes = safe % 60;

            return $"{hours}.{minutes:00}/hours";
        }
    }
}
