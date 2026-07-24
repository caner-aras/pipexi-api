using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Locations;
using Pipexi.Application.Features.OrganizationMembers;
using Pipexi.Application.Features.Shifts.Dtos;
using Pipexi.Application.Features.Teams;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Shifts.Commands.CreateShift;

public sealed record CreateShiftBreakInput(DateTimeOffset StartAt, DateTimeOffset EndAt, bool IsPaid);

public sealed record CreateShiftCommand(
    Guid OrganizationId,
    Guid? TeamId,
    Guid? OrganizationMemberId,
    Guid LocationId,
    string? Title,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string? Notes,
    IReadOnlyCollection<CreateShiftBreakInput>? Breaks,
    IReadOnlyCollection<Guid>? RequiredFormTemplateIds,
    string? Repeat,
    int? RepeatTimes,
    IReadOnlyCollection<int>? RepeatOn,
    int? DayOfMonth) : ICommand<Result<ShiftDto>>
{
    public sealed class Handler : IRequestHandler<CreateShiftCommand, Result<ShiftDto>>
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly IShiftBreakRepository _shiftBreakRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IFormTemplateRepository _formTemplateRepository;
        private readonly IShiftRequiredFormTemplateRepository _shiftRequiredFormTemplateRepository;
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly ITeamMemberDayOffRepository _teamMemberDayOffRepository;

        public Handler(
            IShiftRepository shiftRepository,
            IShiftBreakRepository shiftBreakRepository,
            IOrganizationRepository organizationRepository,
            ITeamRepository teamRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IUserRepository userRepository,
            ILocationRepository locationRepository,
            IFormTemplateRepository formTemplateRepository,
            IShiftRequiredFormTemplateRepository shiftRequiredFormTemplateRepository,
            ITeamMemberRepository teamMemberRepository,
            ITeamMemberDayOffRepository teamMemberDayOffRepository)
        {
            _shiftRepository = shiftRepository;
            _shiftBreakRepository = shiftBreakRepository;
            _organizationRepository = organizationRepository;
            _teamRepository = teamRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _userRepository = userRepository;
            _locationRepository = locationRepository;
            _formTemplateRepository = formTemplateRepository;
            _shiftRequiredFormTemplateRepository = shiftRequiredFormTemplateRepository;
            _teamMemberRepository = teamMemberRepository;
            _teamMemberDayOffRepository = teamMemberDayOffRepository;
        }

        public async Task<Result<ShiftDto>> Handle(CreateShiftCommand request, CancellationToken cancellationToken)
        {
            if (request.StartAt >= request.EndAt)
            {
                return Result<ShiftDto>.Failure(
                    new AppError("shifts.invalid_range", "Shift end time must be after start time."),
                    (int)HttpStatusCode.BadRequest);
            }

            var occurrenceRangesResult = BuildOccurrenceRanges(request);
            if (occurrenceRangesResult.Error is not null)
            {
                return Result<ShiftDto>.Failure(occurrenceRangesResult.Error, (int)HttpStatusCode.BadRequest);
            }

            var occurrenceRanges = occurrenceRangesResult.Ranges;

            var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
            if (organization is null)
            {
                return Result<ShiftDto>.Failure(
                    new AppError("shifts.invalid_organization", "Organization not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var location = await _locationRepository.GetByIdAsync(request.LocationId, cancellationToken);
            if (location is null || location.OrganizationId != request.OrganizationId)
            {
                return Result<ShiftDto>.Failure(
                    new AppError("shifts.invalid_location", "Location not found for organization."),
                    (int)HttpStatusCode.BadRequest);
            }

            Team? team = null;
            OrganizationMember? organizationMember = null;
            if (request.TeamId.HasValue)
            {
                team = await _teamRepository.GetByIdAsync(request.TeamId.Value, cancellationToken);
                if (team is null || team.OrganizationId != request.OrganizationId)
                {
                    return Result<ShiftDto>.Failure(
                        new AppError("shifts.invalid_team", "Team not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            if (request.OrganizationMemberId.HasValue)
            {
                organizationMember = await _organizationMemberRepository.GetByIdAsync(request.OrganizationMemberId.Value, cancellationToken);
                if (organizationMember is null || organizationMember.OrganizationId != request.OrganizationId)
                {
                    return Result<ShiftDto>.Failure(
                        new AppError("shifts.invalid_organization_member", "Organization member not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }

                var teamMemberIds = (await _teamMemberRepository
                        .ListByOrganizationMemberIdAsync(request.OrganizationMemberId.Value, cancellationToken))
                    .Select(x => x.Id)
                    .ToList();

                if (request.TeamId.HasValue)
                {
                    var scopedTeamMemberIds = (await _teamMemberRepository
                            .ListByTeamIdAsync(request.TeamId.Value, cancellationToken))
                        .Where(x => x.OrganizationMemberId == request.OrganizationMemberId.Value)
                        .Select(x => x.Id)
                        .ToHashSet();

                    teamMemberIds = teamMemberIds
                        .Where(scopedTeamMemberIds.Contains)
                        .ToList();
                }

                foreach (var occurrenceRange in occurrenceRanges)
                {
                    var hasDayOffOverlap = await _teamMemberDayOffRepository.HasOverlapForTeamMembersAsync(
                        teamMemberIds,
                        occurrenceRange.StartAt,
                        occurrenceRange.EndAt,
                        cancellationToken);

                    if (hasDayOffOverlap)
                    {
                        return Result<ShiftDto>.Failure(
                            new AppError("shifts.team_member_day_off", "Shift cannot be created because team member is on day off."),
                            (int)HttpStatusCode.Conflict);
                    }
                }
            }

            var requiredFormTemplateIds = (request.RequiredFormTemplateIds ?? Array.Empty<Guid>())
                .Distinct()
                .ToList();

            if (requiredFormTemplateIds.Count > 0)
            {
                var templates = await _formTemplateRepository.GetByIdsAsync(requiredFormTemplateIds, cancellationToken);
                if (templates.Count != requiredFormTemplateIds.Count || templates.Any(x => x.OrganizationId != request.OrganizationId))
                {
                    return Result<ShiftDto>.Failure(
                        new AppError("shifts.invalid_required_form_templates", "One or more required form templates are invalid for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            var breakInputs = request.Breaks ?? Array.Empty<CreateShiftBreakInput>();
            var orderedBreaks = breakInputs.OrderBy(x => x.StartAt).ToList();
            DateTimeOffset? previousBreakEnd = null;
            foreach (var breakInput in orderedBreaks)
            {
                if (breakInput.StartAt >= breakInput.EndAt)
                {
                    return Result<ShiftDto>.Failure(
                        new AppError("shift_breaks.invalid_range", "Break end time must be after start time."),
                        (int)HttpStatusCode.BadRequest);
                }

                if (breakInput.StartAt < request.StartAt || breakInput.EndAt > request.EndAt)
                {
                    return Result<ShiftDto>.Failure(
                        new AppError("shift_breaks.out_of_shift", "Break must be within shift time range."),
                        (int)HttpStatusCode.BadRequest);
                }

                if (previousBreakEnd.HasValue && breakInput.StartAt < previousBreakEnd.Value)
                {
                    return Result<ShiftDto>.Failure(
                        new AppError("shift_breaks.overlap", "Break overlaps with another break."),
                        (int)HttpStatusCode.Conflict);
                }

                previousBreakEnd = breakInput.EndAt;
            }

            var breakOffsets = orderedBreaks
                .Select(x => new
                {
                    StartOffset = x.StartAt - request.StartAt,
                    EndOffset = x.EndAt - request.StartAt,
                    x.IsPaid
                })
                .ToList();

            Shift? firstCreatedShift = null;
            var firstCreatedBreaks = new List<ShiftBreakDto>();
            foreach (var occurrenceRange in occurrenceRanges)
            {
                var shift = Shift.Create(
                    request.OrganizationId,
                    request.TeamId,
                    request.OrganizationMemberId,
                    request.LocationId,
                    request.Title,
                    occurrenceRange.StartAt,
                    occurrenceRange.EndAt,
                    request.Notes);

                await _shiftRepository.AddAsync(shift, cancellationToken);

                foreach (var breakOffset in breakOffsets)
                {
                    var shiftBreak = ShiftBreak.Create(
                        shift.Id,
                        occurrenceRange.StartAt + breakOffset.StartOffset,
                        occurrenceRange.StartAt + breakOffset.EndOffset,
                        breakOffset.IsPaid);

                    await _shiftBreakRepository.AddAsync(shiftBreak, cancellationToken);
                    if (firstCreatedShift is null)
                    {
                        firstCreatedBreaks.Add(shiftBreak.ToDto());
                    }
                }

                if (requiredFormTemplateIds.Count > 0)
                {
                    var requiredTemplates = requiredFormTemplateIds
                        .Select(x => ShiftRequiredFormTemplate.Create(shift.Id, x))
                        .ToList();

                    await _shiftRequiredFormTemplateRepository.AddRangeAsync(requiredTemplates, cancellationToken);
                }

                firstCreatedShift ??= shift;
            }

            if (firstCreatedShift is null)
            {
                return Result<ShiftDto>.Failure(
                    new AppError("shifts.invalid_repeat", "No shift occurrence could be created."),
                    (int)HttpStatusCode.BadRequest);
            }

            var user = organizationMember is null
                ? null
                : await _userRepository.GetByIdAsync(organizationMember.UserId, cancellationToken);

            return Result<ShiftDto>.Success(
                firstCreatedShift.ToDto(
                    team?.ToDto(),
                    organizationMember?.ToDto(user?.ToDto()),
                    location.ToDto(),
                    firstCreatedBreaks),
                (int)HttpStatusCode.Created);
        }

        private static (List<(DateTimeOffset StartAt, DateTimeOffset EndAt)> Ranges, AppError? Error) BuildOccurrenceRanges(CreateShiftCommand request)
        {
            var ranges = new List<(DateTimeOffset StartAt, DateTimeOffset EndAt)>
            {
                (request.StartAt, request.EndAt)
            };

            if (string.IsNullOrWhiteSpace(request.Repeat))
            {
                return (ranges, null);
            }

            var repeatType = request.Repeat.Trim().ToLowerInvariant();
            if (repeatType == "montly")
            {
                repeatType = "monthly";
            }

            if (repeatType is not ("daily" or "weekly" or "monthly"))
            {
                return (ranges, new AppError("shifts.invalid_repeat", "Repeat must be daily, weekly or monthly."));
            }

            if (!request.RepeatTimes.HasValue || request.RepeatTimes.Value <= 0)
            {
                return (ranges, new AppError("shifts.invalid_repeat_times", "RepeatTimes must be greater than 0 when Repeat is provided."));
            }

            var repeatCount = request.RepeatTimes.Value;
            var duration = request.EndAt - request.StartAt;

            switch (repeatType)
            {
                case "daily":
                    {
                        for (var i = 1; i <= repeatCount; i++)
                        {
                            var startAt = request.StartAt.AddDays(i);
                            ranges.Add((startAt, startAt + duration));
                        }

                        break;
                    }

                case "weekly":
                    {
                        var repeatOn = (request.RepeatOn ?? Array.Empty<int>())
                            .Distinct()
                            .OrderBy(x => x)
                            .ToList();

                        if (repeatOn.Count == 0 || repeatOn.Any(x => x < 0 || x > 6))
                        {
                            return (ranges, new AppError("shifts.invalid_repeat_on", "RepeatOn must include values between 0 and 6 for weekly repeat."));
                        }

                        var allowedDays = repeatOn
                            .Select(x => (DayOfWeek)x)
                            .ToHashSet();

                        var currentDate = DateOnly.FromDateTime(request.StartAt.Date.AddDays(1));
                        while (ranges.Count - 1 < repeatCount)
                        {
                            if (allowedDays.Contains(currentDate.DayOfWeek))
                            {
                                var startAt = BuildDateTimeOffset(currentDate, request.StartAt);
                                ranges.Add((startAt, startAt + duration));
                            }

                            currentDate = currentDate.AddDays(1);
                        }

                        break;
                    }

                case "monthly":
                    {
                        if (!request.DayOfMonth.HasValue || request.DayOfMonth.Value < 1 || request.DayOfMonth.Value > 31)
                        {
                            return (ranges, new AppError("shifts.invalid_day_of_month", "DayOfMonth must be between 1 and 31 for monthly repeat."));
                        }

                        for (var i = 1; i <= repeatCount; i++)
                        {
                            var monthDate = DateOnly.FromDateTime(request.StartAt.Date).AddMonths(i);
                            var daysInMonth = DateTime.DaysInMonth(monthDate.Year, monthDate.Month);
                            var day = Math.Min(request.DayOfMonth.Value, daysInMonth);
                            var startAt = BuildDateTimeOffset(new DateOnly(monthDate.Year, monthDate.Month, day), request.StartAt);
                            ranges.Add((startAt, startAt + duration));
                        }

                        break;
                    }
            }

            return (ranges, null);
        }

        private static DateTimeOffset BuildDateTimeOffset(DateOnly date, DateTimeOffset template)
        {
            var dateTime = date.ToDateTime(TimeOnly.MinValue).Add(template.TimeOfDay);
            return new DateTimeOffset(dateTime, template.Offset);
        }
    }
}
